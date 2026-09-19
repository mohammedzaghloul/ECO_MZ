using System.ComponentModel.DataAnnotations;

namespace ECO.BLL.DTO.LandingDtos;

public class TrackLandingEventDto
{
    [Range(1, int.MaxValue)]
    public int LandingPageId { get; set; }

    [Required, StringLength(100, MinimumLength = 8)]
    public string SessionId { get; set; } = string.Empty;

    [Required, RegularExpression("^(visit|form_started|order_submitted)$")]
    public string EventType { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Source { get; set; }
}

public class LandingAnalyticsSummaryDto
{
    public int LandingPageId { get; set; }
    public int Visits { get; set; }
    public int UniqueVisitors { get; set; }
    public int FormStarts { get; set; }
    public int Orders { get; set; }
    public decimal ConversionRate { get; set; }
    public IReadOnlyList<LandingAnalyticsSourceDto> Sources { get; set; } = Array.Empty<LandingAnalyticsSourceDto>();
    public IReadOnlyList<LandingAnalyticsDayDto> Timeline { get; set; } = Array.Empty<LandingAnalyticsDayDto>();
}

public class LandingAnalyticsSourceDto
{
    public string Source { get; set; } = string.Empty;
    public int Visits { get; set; }
}

public class LandingAnalyticsDayDto
{
    public DateTime Date { get; set; }
    public int Visits { get; set; }
    public int Orders { get; set; }
}
