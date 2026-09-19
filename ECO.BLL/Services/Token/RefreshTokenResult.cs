using ECO.DAL.Entities;

namespace ECO.BLL.Services.Token;

public sealed record RefreshTokenResult(
    ApplicationUser User,
    string AccessToken,
    string RefreshToken);
