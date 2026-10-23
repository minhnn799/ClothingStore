using _02._Application.Common;
using _02._Application.Contracts;
using _02._Application.Interfaces;
using _02._Application.IRepository;
using _02._Application.IServices;
using _03._Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;

namespace _03._Infrastructure.Auth;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository
) : IAuthService
{
    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(x => x.Description);

            return string.Join("; ", errors);
        }

        await userManager.AddToRoleAsync(user, "User");

        return "User registered successfully.";
    }

    public async Task<TokenResponse> LoginAsync(
        LoginRequest request)
    {
        var user =
            await userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException(
                "Invalid email or password.");

        var validPassword =
            await userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
            throw new UnauthorizedAccessException(
                "Invalid email or password.");

        var roles =
            await userManager.GetRolesAsync(user);

        var accessToken =
            await jwtTokenService.CreateTokenAsync(
                new JwtTokenRequest(
                    user.Id,
                    user.Email!,
                    roles));

        var refreshToken =
            await refreshTokenRepository.CreateAsync(user.Id);

        return new TokenResponse(
            accessToken,
            refreshToken);
    }

    public async Task<TokenResponse> RefreshAsync(
    RefreshTokenRequest request)
    {
        var refreshToken =
            await refreshTokenRepository.FindAsync(
                request.RefreshToken)
            ?? throw new UserException(
                "Invalid refresh token.");

        var user =
            await userManager.FindByIdAsync(
                refreshToken.UserId)
            ?? throw new UserException(
                "User no longer exists.");

        var roles =
            await userManager.GetRolesAsync(user);

        var accessToken =
            await jwtTokenService.CreateTokenAsync(
                new JwtTokenRequest(
                    user.Id,
                    user.Email!,
                    roles));

        var newRefreshToken =
            await refreshTokenRepository.RotateAsync(
                request.RefreshToken);

        return new TokenResponse(
            accessToken,
            newRefreshToken);
    }

    public async Task LogoutAsync(
        string refreshToken)
    {
        await refreshTokenRepository.RevokeAsync(
            refreshToken);

        await refreshTokenRepository.SaveChangesAsync();
    }
}