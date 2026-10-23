using _02._Application.Contracts;

namespace _02._Application.IServices;

public interface IJwtTokenService
{
    Task<string> CreateTokenAsync(
        JwtTokenRequest request
        );
}