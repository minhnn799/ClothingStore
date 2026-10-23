namespace _04.Domain.Models;

public class ProductVariant
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }

    public string? Cpu { get; set; }
    public string? Gpu { get; set; }

    public int? RamGb { get; set; }
    public int? StorageGb { get; set; }

    public decimal? ScreenSize { get; set; }
    public string? Resolution { get; set; }
    public int? RefreshRate { get; set; }

    public int StockQuantity { get; set; }

    public int? WarrantyMonths { get; set; }

    public bool IsActive { get; set; } = true;
}
