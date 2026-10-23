namespace _04.Domain.Models;

public class ProductImage
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Url { get; set; } = null!;
    public int SortOrder { get; set; }

    public bool IsPrimary { get; set; }
}
