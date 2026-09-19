using ECO.BLL.DTO.LandingDtos;
using ECO.DAL.Data;
using ECO.DAL.Entities.Landing;
using ECO.DAL.Entities.OrderEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
namespace ECO.BLL.Services.LandingSer;

public class LandingAnalyticsService : ILandingAnalyticsService
{
    private static readonly HashSet<string> EventTypes = new(StringComparer.Ordinal)
    {
        "visit", "form_started", "order_submitted"
    };

    private readonly AppDbContext _dbContext;

    public LandingAnalyticsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task TrackAsync(TrackLandingEventDto request, CancellationToken cancellationToken = default)
    {
        var landingExists = await _dbContext.LandingPages
            .AsNoTracking()
            .AnyAsync(page => page.Id == request.LandingPageId && page.IsPublished, cancellationToken);
        if (!landingExists)
            throw new InvalidOperationException("The landing page is not available.");

        var eventType = request.EventType.Trim().ToLowerInvariant();
        if (!EventTypes.Contains(eventType))
            throw new ArgumentException("Unsupported landing event.");

        var sessionId = request.SessionId.Trim();
        var source = string.IsNullOrWhiteSpace(request.Source)
            ? "direct"
            : request.Source.Trim().ToLowerInvariant();

        // A refresh must not create another visit for the same anonymous session.
        if (eventType == "visit" && await _dbContext.LandingPageEvents.AnyAsync(
                analyticsEvent => analyticsEvent.LandingPageId == request.LandingPageId &&
                                  analyticsEvent.SessionId == sessionId &&
                                  analyticsEvent.EventType == eventType,
                cancellationToken))
            return;

        _dbContext.LandingPageEvents.Add(new LandingPageEvent
        {
            LandingPageId = request.LandingPageId,
            SessionId = sessionId,
            EventType = eventType,
            Source = source
        });
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (eventType == "visit" &&
            ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // A concurrent refresh may pass the existence check at the same time.
            // The unique visit index makes the second insert a harmless duplicate.
            _dbContext.Entry(_dbContext.LandingPageEvents.Local.Last()).State = EntityState.Detached;
            return;
        }
    }

    public async Task<LandingAnalyticsSummaryDto> GetSummaryAsync(
        int landingPageId,
        string? period = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.LandingPageEvents
            .AsNoTracking()
            .Where(analyticsEvent => analyticsEvent.LandingPageId == landingPageId);

        var now = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(period))
        {
            switch (period.Trim().ToLowerInvariant())
            {
                case "today":
                    var startOfToday = now.Date;
                    query = query.Where(e => e.CreatedAt >= startOfToday);
                    break;
                case "7d":
                case "7days":
                    var sevenDaysAgo = now.Date.AddDays(-7);
                    query = query.Where(e => e.CreatedAt >= sevenDaysAgo);
                    break;
                case "30d":
                case "30days":
                    var thirtyDaysAgo = now.Date.AddDays(-30);
                    query = query.Where(e => e.CreatedAt >= thirtyDaysAgo);
                    break;
            }
        }
        else
        {
            if (from.HasValue)
                query = query.Where(e => e.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.CreatedAt <= to.Value);
        }

        var visits = await query.CountAsync(analyticsEvent => analyticsEvent.EventType == "visit", cancellationToken);
        var uniqueVisitors = await query
            .Where(analyticsEvent => analyticsEvent.EventType == "visit")
            .Select(analyticsEvent => analyticsEvent.SessionId)
            .Distinct()
            .CountAsync(cancellationToken);
        var formStarts = await query.CountAsync(analyticsEvent => analyticsEvent.EventType == "form_started", cancellationToken);
        var orderQuery = _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.LandingPageId == landingPageId);
        if (!string.IsNullOrWhiteSpace(period))
        {
            var start = period.Trim().ToLowerInvariant() switch
            {
                "today" => DateTime.Now.Date,
                "7d" or "7days" => DateTime.Now.Date.AddDays(-7),
                "30d" or "30days" => DateTime.Now.Date.AddDays(-30),
                _ => (DateTime?)null
            };
            if (start.HasValue)
                orderQuery = orderQuery.Where(order => order.OrderDate >= start.Value);
        }
        else
        {
            if (from.HasValue)
                orderQuery = orderQuery.Where(order => order.OrderDate >= from.Value);
            if (to.HasValue)
                orderQuery = orderQuery.Where(order => order.OrderDate <= to.Value);
        }

        var orders = await orderQuery.CountAsync(cancellationToken);
        var sources = await query
            .Where(analyticsEvent => analyticsEvent.EventType == "visit")
            .GroupBy(analyticsEvent => analyticsEvent.Source)
            .Select(group => new LandingAnalyticsSourceDto
            {
                Source = group.Key,
                Visits = group.Count()
            })
            .OrderByDescending(item => item.Visits)
            .ToListAsync(cancellationToken);
        var timeline = await query
            .GroupBy(analyticsEvent => analyticsEvent.CreatedAt.Date)
            .Select(group => new LandingAnalyticsDayDto
            {
                Date = group.Key,
                Visits = group.Count(item => item.EventType == "visit"),
                Orders = 0
            })
            .OrderBy(item => item.Date)
            .ToListAsync(cancellationToken);
        var orderTimeline = await orderQuery
            .GroupBy(order => order.OrderDate.Date)
            .Select(group => new LandingAnalyticsDayDto
            {
                Date = group.Key,
                Visits = 0,
                Orders = group.Count()
            })
            .ToListAsync(cancellationToken);
        timeline = timeline
            .Concat(orderTimeline)
            .GroupBy(day => day.Date)
            .Select(group => new LandingAnalyticsDayDto
            {
                Date = group.Key,
                Visits = group.Sum(day => day.Visits),
                Orders = group.Sum(day => day.Orders)
            })
            .OrderBy(day => day.Date)
            .ToList();

        return new LandingAnalyticsSummaryDto
        {
            LandingPageId = landingPageId,
            Visits = visits,
            UniqueVisitors = uniqueVisitors,
            FormStarts = formStarts,
            Orders = orders,
            ConversionRate = uniqueVisitors == 0
                ? 0
                : Math.Round(orders * 100m / uniqueVisitors, 2),
            Sources = sources,
            Timeline = timeline
        };
    }
}
