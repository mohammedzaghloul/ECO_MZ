using ECO.Api.Helper;
using ECO.BLL.DTO.LandingDtos;
using ECO.BLL.Services.LandingSer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECO.Api.Controller;

public class LandingAnalyticsController : BaseController
{
    private readonly ILandingAnalyticsService _analyticsService;

    public LandingAnalyticsController(ILandingAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [AllowAnonymous]
    [HttpPost("Track")]
    [EnableRateLimiting("landing-analytics")]
    public async Task<IActionResult> Track(
        TrackLandingEventDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _analyticsService.TrackAsync(request, cancellationToken);
            return Ok(new ResponseApi(200, "Event tracked"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ResponseApi(400, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ResponseApi(404, ex.Message));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{landingPageId:int}/Summary")]
    public async Task<IActionResult> Summary(
        int landingPageId, 
        [FromQuery] string? period, 
        [FromQuery] DateTime? from, 
        [FromQuery] DateTime? to, 
        CancellationToken cancellationToken)
    {
        var summary = await _analyticsService.GetSummaryAsync(landingPageId, period, from, to, cancellationToken);
        return Ok(new GenericResponseApi<LandingAnalyticsSummaryDto>(
            200, "Landing analytics retrieved successfully", summary));
    }
}
