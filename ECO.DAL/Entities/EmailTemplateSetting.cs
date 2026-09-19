namespace ECO.DAL.Entities;

/// <summary>
/// Editable copy for one email template type in one language
/// (order / reset / activation × ar / en). Missing rows fall back
/// to built-in defaults.
/// </summary>
public class EmailTemplateSetting : BaseEntity
{
    public string OwnerEmail { get; set; } = string.Empty;
    public string TemplateType { get; set; } = "order"; // order | reset | activation
    public string Language { get; set; } = "ar";        // ar | en
    public string? Subject { get; set; }
    public string? EyebrowText { get; set; }
    public string? GreetingText { get; set; }
    public string? IntroText { get; set; }
    public string? CtaText { get; set; }
    public string? FooterNote { get; set; }
}
