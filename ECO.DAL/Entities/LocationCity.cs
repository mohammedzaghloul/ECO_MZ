namespace ECO.DAL.Entities;

public class LocationCity : BaseEntity
{
    public string Value { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string Ar { get; set; } = string.Empty;
    public int GovernorateId { get; set; }
    public LocationGovernorate Governorate { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public decimal ShippingPrice { get; set; }
    public int DeliveryDays { get; set; }
    public bool ShippingAvailable { get; set; } = true;
}
