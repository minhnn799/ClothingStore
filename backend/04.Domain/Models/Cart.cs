namespace _04.Domain.Models;

public class Cart
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public ICollection<CartItem> Items { get; set; } = [];
}
