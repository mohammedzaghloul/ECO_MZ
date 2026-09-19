namespace ECO.BLL.DTO.AdminDtos;

public sealed class LocationCityDto
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string Ar { get; set; } = string.Empty;
    public decimal ShippingPrice { get; set; }
    public int DeliveryDays { get; set; }
    public bool ShippingAvailable { get; set; }
}

public sealed class LocationGovernorateDto
{
    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string Ar { get; set; } = string.Empty;
    public IReadOnlyList<LocationCityDto> Cities { get; set; } = [];
}

public sealed class SaveLocationGovernorateDto
{
    public string Value { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string Ar { get; set; } = string.Empty;
}

public sealed class SaveLocationCityDto
{
    public string Value { get; set; } = string.Empty;
    public string En { get; set; } = string.Empty;
    public string Ar { get; set; } = string.Empty;
    public decimal ShippingPrice { get; set; }
    public int DeliveryDays { get; set; }
    public bool ShippingAvailable { get; set; } = true;
}
