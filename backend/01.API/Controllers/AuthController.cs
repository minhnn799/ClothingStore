using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using _01.API.Infrastructure.API;
using _02._Application.Contracts;
using _02._Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace _01.API.Controllers;

[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ApiResponse
{
    [Authorize, HttpGet("me")]
    public IActionResult Me() => Ok(new { email = User.FindFirstValue(ClaimTypes.Email), isAdmin = User.IsInRole("Admin") });

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        return OkResponse(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await authService.LoginAsync(request);

        return OkResponse(token!);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var token = await authService.RefreshAsync(request);

        return OkResponse(token);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request)
    {
        await authService.LogoutAsync(request.RefreshToken);

        return OkResponse("Logged out successfully.");
    }
}