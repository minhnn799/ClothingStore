namespace _04.Domain.Models;

public class PromotionProduct
{
    public Guid PromotionId { get; set; }
    public Promotion Promotion { get; set; } = null!;

    public Guid ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
}
