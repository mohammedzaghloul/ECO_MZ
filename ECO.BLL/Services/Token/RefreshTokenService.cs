using System.Security.Cryptography;
using System.Text;
using ECO.DAL.Data;
using ECO.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ECO.BLL.Services.Token;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly AppDbContext dbContext;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IGenerateToken tokenGenerator;
    private readonly TimeSpan lifetime;

    public RefreshTokenService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IGenerateToken tokenGenerator,
        IConfiguration configuration)
    {
        this.dbContext = dbContext;
        this.userManager = userManager;
        this.tokenGenerator = tokenGenerator;
        var days = configuration.GetValue("Token:RefreshTokenDays", 30);
        lifetime = TimeSpan.FromDays(Math.Max(1, days));
    }

    public async Task<RefreshTokenResult> CreateAsync(ApplicationUser user)
    {
        var rawToken = CreateRawToken();
        dbContext.Set<RefreshToken>().Add(new RefreshToken
        {
            TokenHash = Hash(rawToken),
            UserId = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime)
        });
        await dbContext.SaveChangesAsync();
        return new RefreshTokenResult( user, await tokenGenerator.GenerateTokenAsync(user, userManager), rawToken);
    }

    public async Task<RefreshTokenResult?> RotateAsync(string refreshToken)
    {
        var tokenHash = Hash(refreshToken);
        var storedToken = await dbContext.Set<RefreshToken>()
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash);

        if (storedToken is null ||
            storedToken.RevokedAtUtc.HasValue ||
            storedToken.ExpiresAtUtc <= DateTime.UtcNow)
            return null;

        var replacement = CreateRawToken();
        storedToken.RevokedAtUtc = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = Hash(replacement);
        dbContext.Set<RefreshToken>().Add(new RefreshToken
        {
            TokenHash = storedToken.ReplacedByTokenHash,
            UserId = storedToken.UserId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(lifetime)
        });
        await dbContext.SaveChangesAsync();

        return new RefreshTokenResult(
            storedToken.User,
            await tokenGenerator.GenerateTokenAsync(storedToken.User, userManager),
            replacement);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var token = await dbContext.Set<RefreshToken>()
            .SingleOrDefaultAsync(item => item.TokenHash == Hash(refreshToken));
        if (token is null || token.RevokedAtUtc.HasValue)
            return;

        token.RevokedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    public async Task RevokeAllAsync(string userId)
    {
        var tokens = await dbContext.Set<RefreshToken>()
            .Where(token => token.UserId == userId && !token.RevokedAtUtc.HasValue)
            .ToListAsync();

        if (tokens.Count == 0)
            return;

        var revokedAt = DateTime.UtcNow;
        foreach (var token in tokens)
            token.RevokedAtUtc = revokedAt;

        await dbContext.SaveChangesAsync();
    }

    private static string CreateRawToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
