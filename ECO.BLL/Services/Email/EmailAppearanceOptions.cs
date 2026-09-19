using System.Net;
using System.Text.RegularExpressions;
using ECO.DAL.Entities;

namespace ECO.BLL.Services.Email;

/// <summary>
/// Appearance and copy options for email templates. Read from the StoreSettings
/// row in the database and editable from the admin dashboard.
/// </summary>
public sealed record EmailAppearanceOptions(
    string AccentColor,
    string HeaderStyle,
    string BrandName,
    string? FooterNote,
    string EyebrowText,
    string GreetingText,
    string IntroText,
    string CtaText,
    string BackgroundColor,
    string PaperColor,
    string InkColor,
    string HeadingFont,
    string? LogoUrl)
{
    public static readonly EmailAppearanceOptions Default = new(
        "#2D5A43", "minimal", "ECO", null,
        "تأكيد الطلب", "تم تأكيد طلبك بنجاح", "شكرًا لك، {name}! استلمنا طلبك وهو الآن قيد التجهيز.", "تتبع طلبك",
        "#F6F4EF", "#FFFEFC", "#26251F", "Noto Serif Arabic", null);

    private static readonly Regex HexColor = new("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", RegexOptions.Compiled);

    public static EmailAppearanceOptions From(StoreSettings? settings)
    {
        return new EmailAppearanceOptions(
            NormalizeColor(settings?.EmailAccentColor) ?? Default.AccentColor,
            settings?.EmailHeaderStyle == "accent" ? "accent" : "minimal",
            string.IsNullOrWhiteSpace(settings?.EmailBrandName) ? Default.BrandName : settings!.EmailBrandName!.Trim(),
            string.IsNullOrWhiteSpace(settings?.EmailFooterNote) ? null : settings!.EmailFooterNote!.Trim(),
            Pick(settings?.EmailEyebrowText, Default.EyebrowText),
            Pick(settings?.EmailGreetingText, Default.GreetingText),
            Pick(settings?.EmailIntroText, Default.IntroText),
            Pick(settings?.EmailCtaText, Default.CtaText),
            NormalizeColor(settings?.EmailBackgroundColor) ?? Default.BackgroundColor,
            NormalizeColor(settings?.EmailPaperColor) ?? Default.PaperColor,
            NormalizeColor(settings?.EmailInkColor) ?? Default.InkColor,
            SanitizeFont(settings?.EmailHeadingFont) ?? Default.HeadingFont,
            string.IsNullOrWhiteSpace(settings?.EmailLogoUrl) ? null : settings!.EmailLogoUrl!.Trim());
    }

    private static string Pick(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value!.Trim();

    /// <summary>Returns a normalized #RRGGBB color, or null when the input is not a valid hex color.</summary>
    public static string? NormalizeColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color) || !HexColor.IsMatch(color.Trim()))
            return null;

        var hex = color.Trim().ToUpperInvariant();
        if (hex.Length == 4) // #RGB -> #RRGGBB
            hex = $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
        return hex;
    }

    /// <summary>Strips characters that could break out of a CSS font-family declaration.</summary>
    public static string? SanitizeFont(string? font)
    {
        if (string.IsNullOrWhiteSpace(font))
            return null;
        var cleaned = new string(font.Trim().Where(c => !char.IsWhiteSpace(c) || c == ' ').ToArray());
        cleaned = cleaned.Replace("'", "").Replace("\"", "").Replace(";", "").Replace("{", "").Replace("}", "").Trim();
        return cleaned.Length == 0 || cleaned.Length > 60 ? null : cleaned;
    }

    /// <summary>Replaces {name} and {order} placeholders and HTML-encodes the result.</summary>
    public static string Fill(string template, string customerName, int orderId)
        => WebUtility.HtmlEncode(template
            .Replace("{name}", customerName)
            .Replace("{order}", orderId.ToString()));

    // ---- Derived theme values (templates should read these, not the raw colors) ----

    private (int r, int g, int b) InkRgb => Parse(InkColor);
    private (int r, int g, int b) PaperRgb => Parse(PaperColor);

    /// <summary>Soft text color: ink blended toward paper (secondary copy).</summary>
    public string InkSoft => Blend(InkRgb, PaperRgb, 0.45);

    /// <summary>Faintest text color (copyright line).</summary>
    public string InkFaint => Blend(InkRgb, PaperRgb, 0.66);

    /// <summary>Hairline color: ink blended toward paper.</summary>
    public string LineColor => Blend(InkRgb, PaperRgb, 0.88);

    /// <summary>A darker shade of the accent color for accent text on light backgrounds.</summary>
    public string AccentDeep
    {
        get
        {
            var (r, g, b) = Parse(AccentColor);
            return ToHex((int)Math.Round(r * 0.72), (int)Math.Round(g * 0.72), (int)Math.Round(b * 0.72));
        }
    }

    /// <summary>"r,g,b" components of the accent color, for building rgba() tints in templates.</summary>
    public string AccentRgbRaw
    {
        get
        {
            var (r, g, b) = Parse(AccentColor);
            return $"{r},{g},{b}";
        }
    }

    /// <summary>Semi-transparent accent tint, e.g. rgba(45,90,67,0.08).</summary>
    public string Tint(double alpha) => $"rgba({AccentRgbRaw},{alpha.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)})";

    /// <summary>CSS font stack for headings (editorial serif by default).</summary>
    public string HeadingFontStack => $"'{HeadingFont}', 'Times New Roman', serif";

    /// <summary>CSS font stack for body copy.</summary>
    public const string BodyFontStack = "'IBM Plex Sans Arabic', 'Segoe UI', Tahoma, Arial, sans-serif";

    /// <summary>Google Fonts families to load (heading font + the fixed body font).</summary>
    public string GoogleFontsHref => $"https://fonts.googleapis.com/css2?family={Uri.EscapeDataString(HeadingFont)}:wght@400;500;600;700&family=IBM+Plex+Sans+Arabic:wght@300;400;500;600&display=swap";

    private static string Blend((int r, int g, int b) from, (int r, int g, int b) to, double ratio)
    {
        int Mix(int a, int b) => (int)Math.Round(a + (b - a) * ratio);
        return ToHex(Mix(from.r, to.r), Mix(from.g, to.g), Mix(from.b, to.b));
    }

    private static (int r, int g, int b) Parse(string hex)
        => (Convert.ToInt32(hex.Substring(1, 2), 16), Convert.ToInt32(hex.Substring(3, 2), 16), Convert.ToInt32(hex.Substring(5, 2), 16));

    private static string ToHex(int r, int g, int b)
        => $"#{Math.Clamp(r, 0, 255):X2}{Math.Clamp(g, 0, 255):X2}{Math.Clamp(b, 0, 255):X2}";
}
