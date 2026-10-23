namespace _04.Domain.Models;

public class CartItem
{
    public Guid Id { get; set; }

    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}
