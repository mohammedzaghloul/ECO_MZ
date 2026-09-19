namespace ECO.DAL.Entities;

public class LocationGovernorate : BaseEntity
{
    public string Value { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string Ar { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<LocationCity> Cities { get; set; } = new List<LocationCity>();
}
