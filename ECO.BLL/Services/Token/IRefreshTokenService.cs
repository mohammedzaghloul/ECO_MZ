using ECO.DAL.Entities;

namespace ECO.BLL.Services.Token;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> CreateAsync(ApplicationUser user);
    Task<RefreshTokenResult?> RotateAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
    Task RevokeAllAsync(string userId);
}
