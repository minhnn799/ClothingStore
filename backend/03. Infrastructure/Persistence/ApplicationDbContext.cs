using _03._Infrastructure.Authentication;
using _04.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace _03._Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<RefreshToken>().HasIndex(x => x.TokenHash).IsUnique();
        b.Entity<RefreshToken>().Property(x => x.RevokedAt).IsConcurrencyToken();
        b.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Category>().HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Brand>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Product>().HasIndex(x => x.Sku).IsUnique();
        b.Entity<Product>().HasOne(x => x.Brand).WithMany(x => x.Products).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Product>().HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ProductVariant>().HasIndex(x => x.Sku).IsUnique();
        b.Entity<ProductVariant>().Property(x => x.StockQuantity).IsConcurrencyToken();
        b.Entity<ProductVariant>().ToTable(t => {
            t.HasCheckConstraint("CK_Variant_Stock", "\"StockQuantity\" >= 0");
            t.HasCheckConstraint("CK_Variant_Price", "\"Price\" > 0");
        });
        b.Entity<ProductImage>().HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId);
        b.Entity<Cart>().HasIndex(x => x.UserId).IsUnique();
        b.Entity<Cart>().HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId);
        b.Entity<CartItem>().HasIndex(x => new { x.CartId, x.ProductVariantId }).IsUnique();
        b.Entity<CartItem>().HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<CartItem>().ToTable(t => t.HasCheckConstraint("CK_CartItem_Quantity", "\"Quantity\" > 0"));
        b.Entity<Order>().Property(x => x.Status).HasConversion<string>().IsConcurrencyToken();
        b.Entity<Order>().HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<OrderItem>().HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<PromotionProduct>().HasKey(x => new { x.PromotionId, x.ProductVariantId });
        b.Entity<PromotionProduct>().HasOne(x => x.Promotion).WithMany(x => x.Products).HasForeignKey(x => x.PromotionId);
        b.Entity<PromotionProduct>().HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId);
        b.Entity<Inventory>().HasIndex(x => new { x.WarehouseId, x.ProductVariantId }).IsUnique();
        foreach (var entity in b.Model.GetEntityTypes())
            foreach (var property in entity.GetProperties().Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
    }
}
