using _02._Application.Contracts;

namespace _02._Application.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequest request);

    Task<TokenResponse> LoginAsync(LoginRequest request);

    Task<TokenResponse> RefreshAsync(RefreshTokenRequest request);

    Task LogoutAsync(string refreshToken);
}