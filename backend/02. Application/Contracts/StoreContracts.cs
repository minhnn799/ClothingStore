using System.ComponentModel.DataAnnotations;
namespace _02._Application.Contracts;

public sealed record CategoryRequest(
    [Required, StringLength(120)] string Name,
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$"), StringLength(150)] string Slug,
    Guid? ParentId);
public sealed record BrandRequest(
    [Required, StringLength(120)] string Name,
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$"), StringLength(150)] string Slug);
public sealed record LookupDto(Guid Id, string Name, string? Slug, Guid? ParentId = null);
public sealed record ProductRequest(
    [Required, StringLength(200)] string Name,
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$"), StringLength(200)] string Slug,
    [Required, StringLength(80)] string Sku,
    Guid BrandId, Guid CategoryId,
    [StringLength(8000)] string? Description, bool IsActive = true);
public sealed record VariantRequest(
    [Required, StringLength(80)] string Sku,
    [Required, StringLength(30)] string Size,
    [Required, StringLength(60)] string Color,
    [StringLength(120)] string? Material,
    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal Price,
    [Range(typeof(decimal), "0.01", "999999999", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)] decimal? CompareAtPrice,
    [Range(0, 1000000)] int StockQuantity,
    bool IsActive = true);
public sealed record VariantDto(Guid Id, string Sku, string Size, string Color, string? Material,
    decimal Price, decimal? CompareAtPrice, int StockQuantity, bool IsActive);
public sealed record ImageDto(Guid Id, string Url, int SortOrder, bool IsPrimary);
public sealed record ProductDto(Guid Id, string Name, string Slug, string Sku, string? Description,
    Guid BrandId, string Brand, Guid CategoryId, string Category, bool IsActive,
    IReadOnlyList<VariantDto> Variants, IReadOnlyList<ImageDto> Images);
public sealed record PageDto<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record CartItemRequest(Guid ProductVariantId, [Range(1, 100)] int Quantity);
public sealed record CartLineDto(Guid ProductVariantId, string ProductName, string Sku,
    string Size, string Color, decimal UnitPrice, int Quantity, decimal Total, bool Available);
public sealed record CartDto(IReadOnlyList<CartLineDto> Items, decimal Subtotal);
public sealed record CheckoutRequest(
    [Required, StringLength(150)] string CustomerName,
    [Required, Phone, StringLength(40)] string CustomerPhone,
    [Required, StringLength(1000)] string ShippingAddress);
public sealed record OrderLineDto(Guid ProductVariantId, string ProductName, string Sku,
    decimal UnitPrice, int Quantity, decimal Total);
public sealed record OrderDto(Guid Id, string Status, string CustomerName, string CustomerPhone,
    string ShippingAddress, decimal Subtotal, decimal Discount, decimal ShippingFee, decimal Total,
    DateTime CreatedAt, IReadOnlyList<OrderLineDto> Items);
public sealed record OrderStatusRequest([Required] string Status);
public sealed record StockRequest([Range(0, 1000000)] int Quantity);
