using _02._Application.Common;
using _02._Application.Contracts;
using _02._Application.IRepository;
using _03._Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace _03._Infrastructure.Authentication;

public class RefreshTokenRepository(
    ApplicationDbContext db) : IRefreshTokenRepository
{
    public async Task<string> CreateAsync(string userId)
    {
        var tokenBytes =
            RandomNumberGenerator.GetBytes(64);

        var token =
            Convert.ToBase64String(tokenBytes);

        var tokenHash =
            Convert.ToHexString(
                SHA256.HashData(tokenBytes));

        var refreshToken = new RefreshToken
        {
            TokenHash = tokenHash,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await db.RefreshTokens.AddAsync(refreshToken);
        await db.SaveChangesAsync();

        return token;
    }

    public async Task<RefreshTokenData?> FindAsync(
        string token)
    {
        byte[] tokenBytes;

        try
        {
            tokenBytes =
                Convert.FromBase64String(token);
        }
        catch (FormatException)
        {
            return null;
        }

        var tokenHash =
            Convert.ToHexString(
                SHA256.HashData(tokenBytes));

        var refreshToken =
            await db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == tokenHash &&
                    x.RevokedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow);

        if (refreshToken?.User is null)
            return null;

        return new RefreshTokenData(
            refreshToken.User.Id,
            refreshToken.User.Email!);
    }

    public async Task<string> RotateAsync(
        string token)
    {
        byte[] tokenBytes;

        try
        {
            tokenBytes =
                Convert.FromBase64String(token);
        }
        catch (FormatException)
        {
            throw new UserException(
                "Invalid refresh token.");
        }

        var tokenHash =
            Convert.ToHexString(
                SHA256.HashData(tokenBytes));

        var refreshToken =
            await db.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == tokenHash &&
                    x.RevokedAt == null &&
                    x.ExpiresAt > DateTime.UtcNow);

        if (refreshToken is null)
            throw new UserException(
                "Invalid refresh token.");

        refreshToken.RevokedAt = DateTime.UtcNow;

        return await CreateAsync(refreshToken.UserId);
    }

    public async Task RevokeAsync(
        string token)
    {
        byte[] tokenBytes;

        try
        {
            tokenBytes =
                Convert.FromBase64String(token);
        }
        catch (FormatException)
        {
            return;
        }

        var tokenHash =
            Convert.ToHexString(
                SHA256.HashData(tokenBytes));

        var refreshToken =
            await db.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.TokenHash == tokenHash &&
                    x.RevokedAt == null);

        if (refreshToken is null)
            return;

        refreshToken.RevokedAt = DateTime.UtcNow;
    }

    public Task SaveChangesAsync()
    {
        return db.SaveChangesAsync();
    }
}