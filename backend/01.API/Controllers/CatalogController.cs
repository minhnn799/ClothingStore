using _02._Application.Contracts;
using _02._Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace _01.API.Controllers;

[ApiController, Route("api")]
public sealed class CatalogController(IStoreService store) : ControllerBase
{
    [HttpGet("categories")] public async Task<IActionResult> Categories() => Ok(await store.CategoriesAsync());
    [Authorize(Roles = "Admin"), HttpPost("categories")] public async Task<IActionResult> CreateCategory(CategoryRequest r) => Ok(await store.SaveCategoryAsync(null, r));
    [Authorize(Roles = "Admin"), HttpPut("categories/{id:guid}")] public async Task<IActionResult> UpdateCategory(Guid id, CategoryRequest r) => Ok(await store.SaveCategoryAsync(id, r));
    [Authorize(Roles = "Admin"), HttpDelete("categories/{id:guid}")] public async Task<IActionResult> DeleteCategory(Guid id) { await store.DeleteCategoryAsync(id); return NoContent(); }
    [HttpGet("brands")] public async Task<IActionResult> Brands() => Ok(await store.BrandsAsync());
    [Authorize(Roles = "Admin"), HttpPost("brands")] public async Task<IActionResult> CreateBrand(BrandRequest r) => Ok(await store.SaveBrandAsync(null, r));
    [Authorize(Roles = "Admin"), HttpPut("brands/{id:guid}")] public async Task<IActionResult> UpdateBrand(Guid id, BrandRequest r) => Ok(await store.SaveBrandAsync(id, r));
    [Authorize(Roles = "Admin"), HttpDelete("brands/{id:guid}")] public async Task<IActionResult> DeleteBrand(Guid id) { await store.DeleteBrandAsync(id); return NoContent(); }
    [HttpGet("products")] public async Task<IActionResult> Products(string? search, Guid? categoryId, Guid? brandId, int page = 1, int pageSize = 20) => Ok(await store.ProductsAsync(search, categoryId, brandId, page, pageSize, false));
    [Authorize(Roles = "Admin"), HttpGet("admin/products")] public async Task<IActionResult> AdminProducts(string? search, Guid? categoryId, Guid? brandId, int page = 1, int pageSize = 20) => Ok(await store.ProductsAsync(search, categoryId, brandId, page, pageSize, true));
    [HttpGet("products/{id:guid}")] public async Task<IActionResult> Product(Guid id) => Ok(await store.ProductAsync(id, User.IsInRole("Admin")));
    [Authorize(Roles = "Admin"), HttpPost("products")] public async Task<IActionResult> CreateProduct(ProductRequest r) { var x = await store.SaveProductAsync(null, r); return CreatedAtAction(nameof(Product), new { id = x.Id }, x); }
    [Authorize(Roles = "Admin"), HttpPut("products/{id:guid}")] public async Task<IActionResult> UpdateProduct(Guid id, ProductRequest r) => Ok(await store.SaveProductAsync(id, r));
    [Authorize(Roles = "Admin"), HttpDelete("products/{id:guid}")] public async Task<IActionResult> Archive(Guid id) { await store.ArchiveProductAsync(id); return NoContent(); }
    [Authorize(Roles = "Admin"), HttpPost("products/{productId:guid}/variants")] public async Task<IActionResult> CreateVariant(Guid productId, VariantRequest r) => Ok(await store.SaveVariantAsync(productId, null, r));
    [Authorize(Roles = "Admin"), HttpPut("products/{productId:guid}/variants/{id:guid}")] public async Task<IActionResult> UpdateVariant(Guid productId, Guid id, VariantRequest r) => Ok(await store.SaveVariantAsync(productId, id, r));
    [Authorize(Roles = "Admin"), HttpPut("variants/{id:guid}/stock")] public async Task<IActionResult> Stock(Guid id, StockRequest r) { await store.SetStockAsync(id, r.Quantity); return NoContent(); }
    [Authorize(Roles = "Admin"), HttpPost("products/{productId:guid}/images"), RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> Upload(Guid productId, IFormFile file, [FromForm] bool primary = false)
    {
        if (file.Length == 0 || file.Length > 5_000_000) return BadRequest(new { message = "Image must be 1 byte to 5 MB." });
        await using var stream = file.OpenReadStream();
        return Ok(await store.AddImageAsync(productId, stream, file.FileName, primary));
    }
    [Authorize(Roles = "Admin"), HttpDelete("products/{productId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> DeleteImage(Guid productId, Guid imageId) { await store.DeleteImageAsync(productId, imageId); return NoContent(); }
}
