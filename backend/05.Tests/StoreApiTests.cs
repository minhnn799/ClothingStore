using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
namespace ClothingStore.Tests;

[Collection("PostgreSQL")]
public sealed class StoreApiTests(StoreFixture fixture)
{
    private static async Task<JsonElement> Json(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, response.StatusCode + ": " + body);
        return JsonDocument.Parse(body).RootElement.Clone();
    }
    private async Task<HttpClient> Login(bool admin = false)
    {
        var c = fixture.App.CreateClient();
        var email = admin ? StoreFixture.AdminEmail : Guid.NewGuid() + "@example.test";
        if (!admin) await Json(await c.PostAsJsonAsync("/api/auth/register", new { email, password = StoreFixture.Password }));
        var result = await Json(await c.PostAsJsonAsync("/api/auth/login", new { email, password = StoreFixture.Password }));
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.GetProperty("returnObject").GetProperty("accessToken").GetString());
        return c;
    }
    private async Task<(Guid Product, Guid Variant)> Product(HttpClient admin, int stock = 5)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var category = await Json(await admin.PostAsJsonAsync("/api/categories", new { name = "Shirts", slug = "shirts-" + suffix }));
        var brand = await Json(await admin.PostAsJsonAsync("/api/brands", new { name = "Cotton", slug = "cotton-" + suffix }));
        var p = await Json(await admin.PostAsJsonAsync("/api/products", new { name = "Cotton shirt " + suffix, slug = "shirt-" + suffix, sku = "SH-" + suffix, brandId = brand.GetProperty("id").GetGuid(), categoryId = category.GetProperty("id").GetGuid(), description = "Cotton shirt", isActive = true }));
        var id = p.GetProperty("id").GetGuid();
        var v = await Json(await admin.PostAsJsonAsync($"/api/products/{id}/variants", new { sku = "SKU-" + suffix, size = "M", color = "Blue", material = "Cotton", price = 25.50m, compareAtPrice = 30m, stockQuantity = stock, isActive = true }));
        return (id, v.GetProperty("id").GetGuid());
    }
    private static object Address => new { customerName = "Test Customer", customerPhone = "+84901234567", shippingAddress = "123 Test Street" };
    [Fact]
    public async Task AnonymousAndCustomerCannotManageCatalog()
    {
        using var anonymous = fixture.App.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/cart")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsJsonAsync("/api/brands", new { name = "X", slug = "x" })).StatusCode);
        using var customer = await Login();
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.PostAsJsonAsync("/api/brands", new { name = "X", slug = "x" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/admin/orders")).StatusCode);
    }
    [Fact]
    public async Task InvalidRegistrationAndCredentialsUseErrorStatus()
    {
        using var c = fixture.App.CreateClient();
        var email = Guid.NewGuid() + "@example.test";
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/register", new { email = "invalid", password = "short" })).StatusCode);
        await Json(await c.PostAsJsonAsync("/api/auth/register", new { email, password = StoreFixture.Password }));
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/register", new { email, password = StoreFixture.Password })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await c.PostAsJsonAsync("/api/auth/login", new { email, password = "wrong" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/refresh", new { })).StatusCode);
    }
    [Fact]
    public async Task RefreshRotatesAndLogoutRevokes()
    {
        using var c = fixture.App.CreateClient();
        var result = await Json(await c.PostAsJsonAsync("/api/auth/login", new { email = StoreFixture.AdminEmail, password = StoreFixture.Password }));
        var old = result.GetProperty("returnObject").GetProperty("refreshToken").GetString();
        var renewed = await Json(await c.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = old }));
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = old })).StatusCode);
        var token = renewed.GetProperty("returnObject").GetProperty("refreshToken").GetString();
        await Json(await c.PostAsJsonAsync("/api/auth/logout", new { refreshToken = token }));
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = token })).StatusCode);
    }
    [Fact]
    public async Task CheckoutSnapshotsPricesClearsCartAndCancellationRestocksOnce()
    {
        using var admin = await Login(true); using var c = await Login();
        var (p, v) = await Product(admin);
        await Json(await c.PutAsJsonAsync("/api/cart/items", new { productVariantId = v, quantity = 2 }));
        var order = await Json(await c.PostAsJsonAsync("/api/orders", Address));
        var id = order.GetProperty("id").GetGuid();
        Assert.Equal(51m, order.GetProperty("total").GetDecimal());
        Assert.Equal(0, (await Json(await c.GetAsync("/api/cart"))).GetProperty("items").GetArrayLength());
        var product = await Json(await admin.GetAsync($"/api/products/{p}"));
        Assert.Equal(3, product.GetProperty("variants")[0].GetProperty("stockQuantity").GetInt32());
        var cancelled = await Json(await c.PostAsync($"/api/orders/{id}/cancel", null));
        Assert.Equal("Cancelled", cancelled.GetProperty("status").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsync($"/api/orders/{id}/cancel", null)).StatusCode);
        product = await Json(await admin.GetAsync($"/api/products/{p}"));
        Assert.Equal(5, product.GetProperty("variants")[0].GetProperty("stockQuantity").GetInt32());
    }
    [Fact]
    public async Task OrderOwnershipAndStatusTransitionsAreEnforced()
    {
        using var admin = await Login(true); using var c = await Login(); using var other = await Login();
        var (_, v) = await Product(admin);
        await Json(await c.PutAsJsonAsync("/api/cart/items", new { productVariantId = v, quantity = 1 }));
        var order = await Json(await c.PostAsJsonAsync("/api/orders", Address));
        var id = order.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"/api/orders/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PostAsync($"/api/orders/{id}/cancel", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PutAsJsonAsync($"/api/admin/orders/{id}/status", new { status = "Delivered" })).StatusCode);
        foreach (var status in new[] { "Processing", "Shipped", "Delivered" })
            Assert.Equal(status, (await Json(await admin.PutAsJsonAsync($"/api/admin/orders/{id}/status", new { status }))).GetProperty("status").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsync($"/api/orders/{id}/cancel", null)).StatusCode);
    }
    [Fact]
    public async Task CheckoutRechecksStockAndRollsBackEntireOrder()
    {
        using var admin = await Login(true); using var c = await Login();
        var (p1, v1) = await Product(admin, 2); var (_, v2) = await Product(admin, 2);
        await Json(await c.PutAsJsonAsync("/api/cart/items", new { productVariantId = v1, quantity = 2 }));
        await Json(await c.PutAsJsonAsync("/api/cart/items", new { productVariantId = v2, quantity = 2 }));
        (await admin.PutAsJsonAsync($"/api/variants/{v2}/stock", new { quantity = 0 })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, (await c.PostAsJsonAsync("/api/orders", Address)).StatusCode);
        Assert.Equal(2, (await Json(await admin.GetAsync($"/api/products/{p1}"))).GetProperty("variants")[0].GetProperty("stockQuantity").GetInt32());
        Assert.Equal(2, (await Json(await c.GetAsync("/api/cart"))).GetProperty("items").GetArrayLength());
        Assert.Equal(0, (await Json(await c.GetAsync("/api/orders"))).GetProperty("total").GetInt32());
    }
    [Theory]
    [InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5)]
    public async Task ConcurrentCheckoutCannotOversell(int iteration)
    {
        using var admin = await Login(true); using var a = await Login(); using var b = await Login();
        var (p, v) = await Product(admin, 1);
        await Json(await a.PutAsJsonAsync("/api/cart/items", new { productVariantId = v, quantity = 1 }));
        await Json(await b.PutAsJsonAsync("/api/cart/items", new { productVariantId = v, quantity = 1 }));
        var results = await Task.WhenAll(a.PostAsJsonAsync("/api/orders", Address), b.PostAsJsonAsync("/api/orders", Address));
        Assert.Single(results, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Single(results, x => x.StatusCode == HttpStatusCode.Conflict);
        Assert.Equal(0, (await Json(await admin.GetAsync($"/api/products/{p}"))).GetProperty("variants")[0].GetProperty("stockQuantity").GetInt32());
    }
    [Fact]
    public async Task ValidationArchivingAndCategoryCycleAreEnforced()
    {
        using var admin = await Login(true); using var c = await Login();
        var (p, v) = await Product(admin);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.PutAsJsonAsync("/api/cart/items", new { productVariantId = v, quantity = -1 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsJsonAsync($"/api/variants/{v}/stock", new { quantity = -1 })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await c.GetAsync("/api/products?pageSize=1000")).StatusCode);
        var suffix = Guid.NewGuid().ToString("N");
        var category = await Json(await admin.PostAsJsonAsync("/api/categories", new { name = "Cycle", slug = suffix }));
        var id = category.GetProperty("id").GetGuid();
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PutAsJsonAsync($"/api/categories/{id}", new { name = "Cycle", slug = suffix, parentId = id })).StatusCode);
        (await admin.DeleteAsync($"/api/products/{p}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await c.GetAsync($"/api/products/{p}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await c.PutAsJsonAsync("/api/cart/items", new { productVariantId = v, quantity = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync($"/api/products/{p}")).StatusCode);
    }
    [Fact]
    public async Task ImageUploadRejectsNonImagesAndSupportsDeletion()
    {
        using var admin = await Login(true);
        var (p, _) = await Product(admin);
        using var invalid = new MultipartFormDataContent { { new ByteArrayContent("not an image"u8.ToArray()), "file", "fake.png" } };
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsync($"/api/products/{p}/images", invalid)).StatusCode);
        var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aH1sAAAAASUVORK5CYII=");
        using var content = new MultipartFormDataContent { { new ByteArrayContent(png), "file", "pixel.png" } };
        var image = await Json(await admin.PostAsync($"/api/products/{p}/images", content));
        var url = image.GetProperty("url").GetString()!;
        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(url)).StatusCode);
        (await admin.DeleteAsync($"/api/products/{p}/images/{image.GetProperty("id").GetGuid()}")).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await admin.GetAsync(url)).StatusCode);
    }
    [Fact]
    public async Task OpenApiDocumentAndHealthAreAvailable()
    {
        using var c = fixture.App.CreateClient();
        var doc = await Json(await c.GetAsync("/openapi/v1.json"));
        Assert.True(doc.GetProperty("paths").TryGetProperty("/api/orders", out _));
        Assert.Equal(HttpStatusCode.OK, (await c.GetAsync("/health")).StatusCode);
    }
}
