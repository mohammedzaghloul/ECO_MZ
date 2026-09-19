namespace ECO.BLL.DTO.StoreSettings;

public class SaveEmailAppearanceRequest
{
    // Colors / theme
    public string AccentColor { get; set; } = "#2D5A43";
    public string BackgroundColor { get; set; } = "#F6F4EF";
    public string PaperColor { get; set; } = "#FFFEFC";
    public string InkColor { get; set; } = "#26251F";
    public string HeadingFont { get; set; } = "Noto Serif Arabic";
    public string? LogoUrl { get; set; }

    // Structure
    public string HeaderStyle { get; set; } = "minimal";

    // Brand / copy
    public string BrandName { get; set; } = "ECO";
    public string? FooterNote { get; set; }
    public string? EyebrowText { get; set; }
    public string? GreetingText { get; set; }
    public string? IntroText { get; set; }
    public string? CtaText { get; set; }
}

/// <summary>Test-copy request: same appearance payload (unsaved) + optional recipient + template type.</summary>
public sealed class SendEmailTestRequest : SaveEmailAppearanceRequest
{
    public string? Recipient { get; set; }
    /// <summary>order | activation | reset — defaults to "order"</summary>
    public string TemplateType { get; set; } = "order";
}
