namespace _03._Infrastructure.Authentication;

public class RefreshToken
{
    public int Id { get; set; }

    public string TokenHash { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
