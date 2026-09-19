namespace ECO.BLL.DTO.AdminDtos;

public sealed class LocationCatalogDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string JsonContent { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class SaveLocationCatalogDto
{
    public string Key { get; set; } = "egypt";
    public string JsonContent { get; set; } = string.Empty;
}
