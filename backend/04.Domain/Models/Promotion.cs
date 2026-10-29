namespace _04.Domain.Models;

public class Promotion
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Percentage { get; set; }
    public decimal? Amount { get; set; }

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public bool IsActive { get; set; }

    public ICollection<PromotionProduct> Products { get; set; } = [];
}
