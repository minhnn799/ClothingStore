namespace _04.Domain.Models;

public class Order
{
    public Guid Id { get; set; }

    public string? UserId { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }

    public _04.Domain.Enums.OrderStatus Status { get; set; }

    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string ShippingAddress { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = [];
}
