namespace ECO.DAL.Entities;

public class StoreSettings : BaseEntity
{
    public string OwnerEmail { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "EGP";
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPasswordProtected { get; set; }
    public string? SmtpFrom { get; set; }
    public bool SmtpUseSsl { get; set; } = true;

    // Email template appearance (controlled from the admin dashboard).
    public string EmailAccentColor { get; set; } = "#2D5A43";
    public string EmailHeaderStyle { get; set; } = "minimal"; // minimal | accent
    public string EmailBrandName { get; set; } = "ECO";
    public string? EmailFooterNote { get; set; }

    // Theme colors/fonts (single settings row, read by every email template).
    public string EmailBackgroundColor { get; set; } = "#F6F4EF";
    public string EmailPaperColor { get; set; } = "#FFFEFC";
    public string EmailInkColor { get; set; } = "#26251F";
    public string EmailHeadingFont { get; set; } = "Noto Serif Arabic";
    public string? EmailLogoUrl { get; set; }
    public string EmailDefaultLanguage { get; set; } = "ar"; // ar | en

    // Editable email copy (supports {name} and {order} placeholders).
    public string? EmailEyebrowText { get; set; }   // default: تأكيد الطلب
    public string? EmailGreetingText { get; set; }  // default: شكرًا لثقتك، {name}
    public string? EmailIntroText { get; set; }     // default: طلبك بين أيدينا الآن...
    public string? EmailCtaText { get; set; }       // default: متابعة تفاصيل الطلب
}
