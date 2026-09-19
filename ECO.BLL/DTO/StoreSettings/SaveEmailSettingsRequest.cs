namespace ECO.BLL.DTO.StoreSettings;

public sealed class SaveEmailSettingsRequest
{
    public string SmtpHost { get; set; } = string.Empty;
    public int? SmtpPort { get; set; } = 465;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpFrom { get; set; } = string.Empty;
    public string? Password { get; set; }
    public bool SmtpUseSsl { get; set; } = true;
}
