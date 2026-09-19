namespace ECO.DAL.Entities;

public class LocationCatalog : BaseEntity
{
    public string Key { get; set; } = "egypt";
    public string JsonContent { get; set; } = "{}";
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
