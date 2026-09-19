namespace ECO.DAL.Entities.Landing;

public class LandingPageEvent : BaseEntity
{
    public int LandingPageId { get; set; }
    public LandingPage LandingPage { get; set; } = null!;
    public string SessionId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Source { get; set; } = "direct";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
