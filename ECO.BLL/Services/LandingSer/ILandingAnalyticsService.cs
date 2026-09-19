using ECO.BLL.DTO.LandingDtos;

namespace ECO.BLL.Services.LandingSer;

public interface ILandingAnalyticsService
{
    Task TrackAsync(TrackLandingEventDto request, CancellationToken cancellationToken = default);
    Task<LandingAnalyticsSummaryDto> GetSummaryAsync(
        int landingPageId, 
        string? period = null, 
        DateTime? from = null, 
        DateTime? to = null, 
        CancellationToken cancellationToken = default);
}
