namespace _04.Domain.Models;

public class Brand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
