using _02._Application.Contracts;
using _02._Application.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace _03._Infrastructure.Authentication;

public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{

    public Task<string> CreateTokenAsync(JwtTokenRequest request)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId),
            new(JwtRegisteredClaimNames.Email, request.Email),

            new(ClaimTypes.NameIdentifier, request.UserId),
            new(ClaimTypes.Name, request.Email),
            new(ClaimTypes.Email, request.Email)
        };

        claims.AddRange(
            request.Roles.Select(role =>
                new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            //expires: DateTime.UtcNow.AddHours(1),
            expires: DateTime.UtcNow.AddMinutes(int.Parse(configuration["Jwt:ExpirationMinutes"]!)),
            signingCredentials: credentials);

        return Task.FromResult(
            new JwtSecurityTokenHandler().WriteToken(token));
    }
}