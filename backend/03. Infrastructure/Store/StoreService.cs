using System.Data;
using _02._Application.Common;
using _02._Application.Contracts;
using _02._Application.IServices;
using _02._Application.Storage;
using _03._Infrastructure.Persistence;
using _04.Domain.Enums;
using _04.Domain.Models;
using Microsoft.EntityFrameworkCore;
namespace _03._Infrastructure.Store;

public sealed class StoreService(ApplicationDbContext db, IFileStorage storage) : IStoreService
{
    private static StoreException Missing() => new("Resource not found.", 404);
    private static void Page(int page, int size) { if (page < 1 || page > 100000 || size < 1 || size > 100) throw new StoreException("Page must be 1–100000 and pageSize 1–100."); }
    public async Task<IReadOnlyList<LookupDto>> CategoriesAsync() => await db.Categories.AsNoTracking().OrderBy(x => x.Name).Select(x => new LookupDto(x.Id, x.Name, x.Slug, x.ParentId)).ToListAsync();
    public async Task<LookupDto> SaveCategoryAsync(Guid? id, CategoryRequest r)
    {
        var x = id.HasValue ? await db.Categories.FindAsync(id.Value) ?? throw Missing() : new Category { Id = Guid.NewGuid() };
        if (await db.Categories.AnyAsync(c => c.Slug == r.Slug && c.Id != x.Id)) throw new StoreException("Category slug already exists.", 409);
        var parentId = r.ParentId;
        var seen = new HashSet<Guid> { x.Id };
        while (parentId.HasValue)
        {
            if (!seen.Add(parentId.Value)) throw new StoreException("Category hierarchy cannot contain a cycle.");
            var parent = await db.Categories.FindAsync(parentId.Value) ?? throw new StoreException("Parent category does not exist.");
            parentId = parent.ParentId;
        }
        x.Name = r.Name.Trim(); x.Slug = r.Slug; x.ParentId = r.ParentId;
        if (!id.HasValue) db.Categories.Add(x);
        await db.SaveChangesAsync();
        return new(x.Id, x.Name, x.Slug, x.ParentId);
    }
    public async Task DeleteCategoryAsync(Guid id)
    {
        var x = await db.Categories.FindAsync(id) ?? throw Missing();
        if (await db.Products.AnyAsync(p => p.CategoryId == id) || await db.Categories.AnyAsync(c => c.ParentId == id)) throw new StoreException("Category is in use.", 409);
        db.Categories.Remove(x); await db.SaveChangesAsync();
    }
    public async Task<IReadOnlyList<LookupDto>> BrandsAsync() => await db.Brands.AsNoTracking().OrderBy(x => x.Name).Select(x => new LookupDto(x.Id, x.Name, x.Slug)).ToListAsync();
    public async Task<LookupDto> SaveBrandAsync(Guid? id, BrandRequest r)
    {
        var x = id.HasValue ? await db.Brands.FindAsync(id.Value) ?? throw Missing() : new Brand { Id = Guid.NewGuid() };
        if (await db.Brands.AnyAsync(c => c.Slug == r.Slug && c.Id != x.Id)) throw new StoreException("Brand slug already exists.", 409);
        x.Name = r.Name.Trim(); x.Slug = r.Slug;
        if (!id.HasValue) db.Brands.Add(x);
        await db.SaveChangesAsync(); return new(x.Id, x.Name, x.Slug);
    }
    public async Task DeleteBrandAsync(Guid id)
    {
        var x = await db.Brands.FindAsync(id) ?? throw Missing();
        if (await db.Products.AnyAsync(p => p.BrandId == id)) throw new StoreException("Brand is in use.", 409);
        db.Brands.Remove(x); await db.SaveChangesAsync();
    }
    private IQueryable<Product> ProductQuery() => db.Products.Include(x => x.Brand).Include(x => x.Category).Include(x => x.Variants).Include(x => x.Images).AsSplitQuery();
    public async Task<PageDto<ProductDto>> ProductsAsync(string? search, Guid? categoryId, Guid? brandId, int page, int pageSize, bool includeInactive)
    {
        Page(page, pageSize);
        var q = ProductQuery().AsNoTracking();
        if (!includeInactive) q = q.Where(x => x.IsActive);
        if (categoryId.HasValue) q = q.Where(x => x.CategoryId == categoryId);
        if (brandId.HasValue) q = q.Where(x => x.BrandId == brandId);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim().ToLower(); q = q.Where(x => x.Name.ToLower().Contains(term) || x.Sku.ToLower().Contains(term)); }
        var total = await q.CountAsync();
        var items = await q.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new(items.Select(x => Map(x, includeInactive)).ToList(), total, page, pageSize);
    }
    public async Task<ProductDto> ProductAsync(Guid id, bool includeInactive = false)
    {
        var x = await ProductQuery().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && (includeInactive || x.IsActive)) ?? throw Missing();
        return Map(x, includeInactive);
    }
    public async Task<ProductDto> SaveProductAsync(Guid? id, ProductRequest r)
    {
        if (!await db.Brands.AnyAsync(x => x.Id == r.BrandId) || !await db.Categories.AnyAsync(x => x.Id == r.CategoryId)) throw new StoreException("Brand and category must exist.");
        var x = id.HasValue ? await db.Products.FindAsync(id.Value) ?? throw Missing() : new Product { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        if (await db.Products.AnyAsync(p => p.Id != x.Id && (p.Slug == r.Slug || p.Sku == r.Sku))) throw new StoreException("Product slug or SKU already exists.", 409);
        x.Name = r.Name.Trim(); x.Slug = r.Slug; x.Sku = r.Sku.Trim(); x.BrandId = r.BrandId; x.CategoryId = r.CategoryId; x.Description = r.Description; x.IsActive = r.IsActive; x.UpdatedAt = DateTime.UtcNow;
        if (!id.HasValue) db.Products.Add(x);
        await db.SaveChangesAsync(); return await ProductAsync(x.Id, true);
    }
    public async Task ArchiveProductAsync(Guid id)
    {
        var x = await db.Products.FindAsync(id) ?? throw Missing();
        x.IsActive = false; x.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync();
    }
    public async Task<VariantDto> SaveVariantAsync(Guid productId, Guid? id, VariantRequest r)
    {
        if (!await db.Products.AnyAsync(x => x.Id == productId)) throw Missing();
        if (r.Price <= 0 || r.StockQuantity < 0 || (r.CompareAtPrice.HasValue && r.CompareAtPrice < r.Price)) throw new StoreException("Price and stock are invalid, or compare-at price is below price.");
        var x = id.HasValue ? await db.ProductVariants.SingleOrDefaultAsync(x => x.Id == id && x.ProductId == productId) ?? throw Missing() : new ProductVariant { Id = Guid.NewGuid(), ProductId = productId };
        if (await db.ProductVariants.AnyAsync(v => v.Id != x.Id && v.Sku == r.Sku)) throw new StoreException("Variant SKU already exists.", 409);
        x.Sku = r.Sku.Trim(); x.Size = r.Size.Trim(); x.Color = r.Color.Trim(); x.Material = r.Material; x.Price = decimal.Round(r.Price, 2); x.CompareAtPrice = r.CompareAtPrice.HasValue ? decimal.Round(r.CompareAtPrice.Value, 2) : null; x.StockQuantity = r.StockQuantity; x.IsActive = r.IsActive;
        if (!id.HasValue) db.ProductVariants.Add(x);
        await db.SaveChangesAsync(); return Map(x);
    }
    public async Task SetStockAsync(Guid id, int quantity)
    {
        if (quantity < 0) throw new StoreException("Stock cannot be negative.");
        var x = await db.ProductVariants.FindAsync(id) ?? throw Missing();
        x.StockQuantity = quantity; await db.SaveChangesAsync();
    }
    public async Task<ImageDto> AddImageAsync(Guid productId, Stream file, string fileName, bool primary)
    {
        var p = await db.Products.Include(x => x.Images).SingleOrDefaultAsync(x => x.Id == productId) ?? throw Missing();
        var upload = await storage.UploadAsync(file, fileName, "products");
        var x = new ProductImage { Id = Guid.NewGuid(), ProductId = productId, Url = upload.Url, PublicId = upload.PublicId, SortOrder = p.Images.Count, IsPrimary = primary || p.Images.Count == 0 };
        if (x.IsPrimary) foreach (var old in p.Images) old.IsPrimary = false;
        p.Images.Add(x); db.Add(x);
        try { await db.SaveChangesAsync(); }
        catch { await storage.DeleteAsync(upload.PublicId); throw; }
        return new(x.Id, x.Url, x.SortOrder, x.IsPrimary);
    }
    public async Task DeleteImageAsync(Guid productId, Guid imageId)
    {
        var p = await db.Products.Include(x => x.Images).SingleOrDefaultAsync(x => x.Id == productId) ?? throw Missing();
        var x = p.Images.SingleOrDefault(x => x.Id == imageId) ?? throw Missing();
        if (x.PublicId is not null) await storage.DeleteAsync(x.PublicId);
        p.Images.Remove(x); db.Remove(x);
        if (x.IsPrimary && p.Images.Count > 0) p.Images.OrderBy(x => x.SortOrder).First().IsPrimary = true;
        await db.SaveChangesAsync();
    }
    public async Task<CartDto> CartAsync(string userId)
    {
        var cart = await db.Carts.AsNoTracking().Include(x => x.Items).SingleOrDefaultAsync(x => x.UserId == userId);
        if (cart is null) return new([], 0);
        var ids = cart.Items.Select(x => x.ProductVariantId).ToList();
        var variants = await db.ProductVariants.AsNoTracking().Include(x => x.Product).Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        var lines = cart.Items.Select(x => { var v = variants[x.ProductVariantId]; return new CartLineDto(v.Id, v.Product.Name, v.Sku, v.Size, v.Color, v.Price, x.Quantity, v.Price * x.Quantity, v.IsActive && v.Product.IsActive && v.StockQuantity >= x.Quantity); }).ToList();
        return new(lines, lines.Sum(x => x.Total));
    }
    public async Task<CartDto> SetCartItemAsync(string userId, CartItemRequest r)
    {
        if (r.Quantity < 1 || r.Quantity > 100) throw new StoreException("Quantity must be between 1 and 100.");
        var v = await db.ProductVariants.Include(x => x.Product).SingleOrDefaultAsync(x => x.Id == r.ProductVariantId && x.IsActive && x.Product.IsActive) ?? throw Missing();
        if (r.Quantity > v.StockQuantity) throw new StoreException("Insufficient stock.", 409);
        var cart = await db.Carts.Include(x => x.Items).SingleOrDefaultAsync(x => x.UserId == userId);
        if (cart is null) { cart = new Cart { Id = Guid.NewGuid(), UserId = userId }; db.Carts.Add(cart); }
        var item = cart.Items.SingleOrDefault(x => x.ProductVariantId == r.ProductVariantId);
        if (item is null) { item = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductVariantId = v.Id, Quantity = r.Quantity }; cart.Items.Add(item); db.Add(item); }
        else item.Quantity = r.Quantity;
        await db.SaveChangesAsync(); return await CartAsync(userId);
    }
    public async Task RemoveCartItemAsync(string userId, Guid variantId)
    {
        var item = await db.Set<CartItem>().SingleOrDefaultAsync(x => x.Cart.UserId == userId && x.ProductVariantId == variantId);
        if (item is not null) { db.Remove(item); await db.SaveChangesAsync(); }
    }
    public async Task<OrderDto> CheckoutAsync(string userId, CheckoutRequest r)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var cart = await db.Carts.Include(x => x.Items).SingleOrDefaultAsync(x => x.UserId == userId);
        if (cart is null || cart.Items.Count == 0) throw new StoreException("Cart is empty.");
        var ids = cart.Items.Select(x => x.ProductVariantId).ToList();
        var variants = await db.ProductVariants.Include(x => x.Product).Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        var order = new Order { Id = Guid.NewGuid(), UserId = userId, CustomerName = r.CustomerName.Trim(), CustomerPhone = r.CustomerPhone.Trim(), ShippingAddress = r.ShippingAddress.Trim(), CreatedAt = DateTime.UtcNow, Status = OrderStatus.Pending };
        foreach (var item in cart.Items)
        {
            if (!variants.TryGetValue(item.ProductVariantId, out var v) || !v.IsActive || !v.Product.IsActive || v.StockQuantity < item.Quantity) throw new StoreException("An item is unavailable or has insufficient stock.", 409);
            v.StockQuantity -= item.Quantity;
            order.Items.Add(new OrderItem { Id = Guid.NewGuid(), ProductVariantId = v.Id, ProductName = v.Product.Name + " (" + v.Size + ", " + v.Color + ")", Sku = v.Sku, UnitPrice = v.Price, Quantity = item.Quantity, Total = v.Price * item.Quantity });
        }
        order.Subtotal = order.Items.Sum(x => x.Total);
        order.Total = order.Subtotal; // Cash on delivery; shipping is free for the initial store.
        db.Orders.Add(order); db.RemoveRange(cart.Items);
        await db.SaveChangesAsync(); await tx.CommitAsync(); return Map(order);
    }
    public async Task<PageDto<OrderDto>> OrdersAsync(string? userId, int page, int pageSize)
    {
        Page(page, pageSize);
        var q = db.Orders.AsNoTracking().Include(x => x.Items).Where(x => userId == null || x.UserId == userId);
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new(items.Select(Map).ToList(), total, page, pageSize);
    }
    public async Task<OrderDto> OrderAsync(Guid id, string? userId) => Map(await db.Orders.AsNoTracking().Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id && (userId == null || x.UserId == userId)) ?? throw Missing());
    public async Task<OrderDto> ChangeOrderStatusAsync(Guid id, string status, string? userId = null)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var next) || !Enum.IsDefined(next)) throw new StoreException("Invalid order status.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var order = await db.Orders.Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id && (userId == null || x.UserId == userId)) ?? throw Missing();
        if (userId is not null && (next != OrderStatus.Cancelled || order.Status != OrderStatus.Pending)) throw new StoreException("Only pending orders can be cancelled by the customer.", 409);
        var allowed = (order.Status, next) switch {
            (OrderStatus.Pending, OrderStatus.Processing or OrderStatus.Cancelled) => true,
            (OrderStatus.Processing, OrderStatus.Shipped or OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            _ => false
        };
        if (!allowed) throw new StoreException("Order status transition is not allowed.", 409);
        if (next == OrderStatus.Cancelled)
            foreach (var item in order.Items)
            {
                var variant = await db.ProductVariants.FindAsync(item.ProductVariantId) ?? throw Missing();
                variant.StockQuantity += item.Quantity;
            }
        order.Status = next;
        await db.SaveChangesAsync(); await tx.CommitAsync(); return Map(order);
    }
    private static VariantDto Map(ProductVariant x) => new(x.Id, x.Sku, x.Size, x.Color, x.Material, x.Price, x.CompareAtPrice, x.StockQuantity, x.IsActive);
    private static ProductDto Map(Product x, bool includeInactive) => new(x.Id, x.Name, x.Slug, x.Sku, x.Description, x.BrandId, x.Brand.Name, x.CategoryId, x.Category.Name, x.IsActive, x.Variants.Where(v => includeInactive || v.IsActive).OrderBy(v => v.Sku).Select(Map).ToList(), x.Images.OrderBy(i => i.SortOrder).Select(i => new ImageDto(i.Id, i.Url, i.SortOrder, i.IsPrimary)).ToList());
    private static OrderDto Map(Order x) => new(x.Id, x.Status.ToString(), x.CustomerName, x.CustomerPhone, x.ShippingAddress, x.Subtotal, x.Discount, x.ShippingFee, x.Total, x.CreatedAt, x.Items.Select(i => new OrderLineDto(i.ProductVariantId, i.ProductName, i.Sku, i.UnitPrice, i.Quantity, i.Total)).ToList());
}
