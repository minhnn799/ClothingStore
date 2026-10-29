using _02._Application.Contracts;

namespace _02._Application.IRepository;

public interface IRefreshTokenRepository
{
    Task<string> CreateAsync(string userId);

    Task<RefreshTokenData?> FindAsync(
        string token);

    Task<string> RotateAsync(
        string token);

    Task RevokeAsync(
        string token);

    Task SaveChangesAsync();
}
