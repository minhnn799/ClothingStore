using _02._Application.Contracts;
namespace _02._Application.IServices;
public interface IStoreService
{
    Task<IReadOnlyList<LookupDto>> CategoriesAsync();
    Task<LookupDto> SaveCategoryAsync(Guid? id, CategoryRequest request);
    Task DeleteCategoryAsync(Guid id);
    Task<IReadOnlyList<LookupDto>> BrandsAsync();
    Task<LookupDto> SaveBrandAsync(Guid? id, BrandRequest request);
    Task DeleteBrandAsync(Guid id);
    Task<PageDto<ProductDto>> ProductsAsync(string? search, Guid? categoryId, Guid? brandId, int page, int pageSize, bool includeInactive);
    Task<ProductDto> ProductAsync(Guid id, bool includeInactive = false);
    Task<ProductDto> SaveProductAsync(Guid? id, ProductRequest request);
    Task ArchiveProductAsync(Guid id);
    Task<VariantDto> SaveVariantAsync(Guid productId, Guid? id, VariantRequest request);
    Task SetStockAsync(Guid id, int quantity);
    Task<ImageDto> AddImageAsync(Guid productId, Stream file, string fileName, bool primary);
    Task DeleteImageAsync(Guid productId, Guid imageId);
    Task<CartDto> CartAsync(string userId);
    Task<CartDto> SetCartItemAsync(string userId, CartItemRequest request);
    Task RemoveCartItemAsync(string userId, Guid variantId);
    Task<OrderDto> CheckoutAsync(string userId, CheckoutRequest request);
    Task<PageDto<OrderDto>> OrdersAsync(string? userId, int page, int pageSize);
    Task<OrderDto> OrderAsync(Guid id, string? userId);
    Task<OrderDto> ChangeOrderStatusAsync(Guid id, string status, string? userId = null);
}
