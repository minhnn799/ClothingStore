namespace _04.Domain.Models;
public class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Size { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string? Material { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}
