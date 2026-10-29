namespace _04.Domain.Models;

public class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductVariantId { get; set; }

    public string ProductName { get; set; } = null!;
    public string Sku { get; set; } = null!;

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public decimal Total { get; set; }
}