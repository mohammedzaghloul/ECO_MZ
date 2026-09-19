using System.Security.Claims;
using ECO.BLL.DTO.Auth;
using ECO.BLL.DTO.StoreSettings;
using ECO.BLL.Services.Email;
using ECO.DAL.Data;
using ECO.DAL.Entities;
using ECO.DAL.Entities.OrderEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECO.Api.Controller;

/// <summary>
/// Admin email designer: read/save the email appearance (colors, fonts, brand, copy),
/// preview the order template, and send a test copy using the edited (unsaved) values.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class EmailSettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly EmailAppearanceService _emailAppearanceService;

    public EmailSettingsController(
        AppDbContext db,
        IEmailService emailService,
        IConfiguration configuration,
        EmailAppearanceService emailAppearanceService)
    {
        _db = db;
        _emailService = emailService;
        _configuration = configuration;
        _emailAppearanceService = emailAppearanceService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        var look = await LoadOwnerAppearanceAsync(token);
        return Ok(AppearanceResponse(look));
    }

    [HttpPut]
    public async Task<IActionResult> Save([FromBody] SaveEmailAppearanceRequest request, CancellationToken token)
    {
        if (!TryValidateAppearance(request, out var accent, out var brand, out var footerNote,
                out var eyebrowText, out var greetingText, out var introText, out var ctaText,
                out var error))
            return BadRequest(new { message = error });

        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var settings = await _db.StoreSettings.FirstOrDefaultAsync(x => x.OwnerEmail == email, token);
        if (settings is null)
        {
            settings = new StoreSettings { OwnerEmail = email };
            await _db.StoreSettings.AddAsync(settings, token);
        }

        settings.EmailAccentColor = accent!;
        settings.EmailHeaderStyle = request.HeaderStyle;
        settings.EmailBrandName = brand!;
        settings.EmailFooterNote = footerNote;
        settings.EmailEyebrowText = eyebrowText;
        settings.EmailGreetingText = greetingText;
        settings.EmailIntroText = introText;
        settings.EmailCtaText = ctaText;
        settings.EmailBackgroundColor = request.BackgroundColor.Trim().ToUpperInvariant();
        settings.EmailPaperColor = request.PaperColor.Trim().ToUpperInvariant();
        settings.EmailInkColor = request.InkColor.Trim().ToUpperInvariant();
        settings.EmailHeadingFont = EmailAppearanceOptions.SanitizeFont(request.HeadingFont) ?? EmailAppearanceOptions.Default.HeadingFont;
        settings.EmailLogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl!.Trim();

        await _db.SaveChangesAsync(token);

        var look = EmailAppearanceOptions.From(settings);
        return Ok(AppearanceResponse(look));
    }

    /// <summary>Renders an email template (order, activation, or reset) with sample data and the saved appearance.</summary>
    [HttpGet("preview")]
    [Produces("text/html")]
    public async Task<IActionResult> Preview([FromQuery] string type = "order", CancellationToken token = default)
    {
        var look = await LoadOwnerAppearanceAsync(token);
        return await RenderTemplateContentAsync(look, type, token);
    }

    /// <summary>Renders an email template with unsaved appearance (for live preview of edits, logo, etc).</summary>
    [HttpPost("preview")]
    [Produces("text/html")]
    public async Task<IActionResult> PreviewLive([FromBody] SaveEmailAppearanceRequest request, [FromQuery] string type = "order", CancellationToken token = default)
    {
        var look = EmailAppearanceOptions.From(new StoreSettings
        {
            EmailAccentColor = request.AccentColor,
            EmailHeaderStyle = request.HeaderStyle,
            EmailBrandName = string.IsNullOrWhiteSpace(request.BrandName) ? "ECO" : request.BrandName.Trim(),
            EmailFooterNote = request.FooterNote,
            EmailEyebrowText = request.EyebrowText,
            EmailGreetingText = request.GreetingText,
            EmailIntroText = request.IntroText,
            EmailCtaText = request.CtaText,
            EmailBackgroundColor = request.BackgroundColor,
            EmailPaperColor = request.PaperColor,
            EmailInkColor = request.InkColor,
            EmailHeadingFont = request.HeadingFont,
            EmailLogoUrl = request.LogoUrl
        });

        return await RenderTemplateContentAsync(look, type, token);
    }

    private async Task<IActionResult> RenderTemplateContentAsync(EmailAppearanceOptions look, string type, CancellationToken token)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";

        if (string.Equals(type, "activation", StringComparison.OrdinalIgnoreCase))
        {
            var copy = await _emailAppearanceService.GetCopyAsync(EmailTemplateCopy.Activation, null, token);
            var activationHtml = AccountActivationEmailTemplate.Build(look, copy, $"{frontendUrl}/account/confirm-email?token=SAMPLE", 60, "محمد أحمد");
            return Content(activationHtml, "text/html; charset=utf-8");
        }

        if (string.Equals(type, "reset", StringComparison.OrdinalIgnoreCase))
        {
            var copy = await _emailAppearanceService.GetCopyAsync(EmailTemplateCopy.Reset, null, token);
            var resetHtml = ResetPasswordEmailTemplate.Build(look, copy, $"{frontendUrl}/account/reset-password?token=SAMPLE", 30, "محمد أحمد");
            return Content(resetHtml, "text/html; charset=utf-8");
        }

        var orderHtml = OrderEmailTemplate.Build(BuildSampleOrder(), frontendUrl, look);
        return Content(orderHtml, "text/html; charset=utf-8");
    }

    /// <summary>Sends a test copy rendered with the currently edited (unsaved) appearance.</summary>
    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromBody] SendEmailTestRequest request, CancellationToken token)
    {
        if (!TryValidateAppearance(request, out var accent, out var brand, out var footerNote,
                out var eyebrowText, out var greetingText, out var introText, out var ctaText,
                out var error))
            return BadRequest(new { message = error });

        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var settings = await _db.StoreSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerEmail == email, token);
        var recipient = string.IsNullOrWhiteSpace(request.Recipient)
            ? settings?.SmtpFrom
            : request.Recipient!.Trim();
        if (string.IsNullOrWhiteSpace(recipient))
            return BadRequest(new { message = "حدد بريد المستلم أولًا، أو احفظ بريد المرسل في إعدادات SMTP." });

        // Unsaved appearance straight from the request body.
        var look = EmailAppearanceOptions.From(new StoreSettings
        {
            EmailAccentColor = request.AccentColor,
            EmailHeaderStyle = request.HeaderStyle,
            EmailBrandName = request.BrandName,
            EmailFooterNote = footerNote,
            EmailEyebrowText = eyebrowText,
            EmailGreetingText = greetingText,
            EmailIntroText = introText,
            EmailCtaText = ctaText,
            EmailBackgroundColor = request.BackgroundColor,
            EmailPaperColor = request.PaperColor,
            EmailInkColor = request.InkColor,
            EmailHeadingFont = request.HeadingFont,
            EmailLogoUrl = request.LogoUrl
        });

        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:4200";

        // Render the same template type the admin is currently previewing.
        string templateType = (request.TemplateType ?? "order").Trim().ToLowerInvariant();
        string subject;
        string html;

        if (templateType == "activation")
        {
            var copy = await _emailAppearanceService.GetCopyAsync(EmailTemplateCopy.Activation, null, token);
            html = AccountActivationEmailTemplate.Build(look, copy, $"{frontendUrl}/account/confirm-email?token=SAMPLE", 60, "محمد أحمد");
            subject = $"تفعيل الحساب | {look.BrandName}";
        }
        else if (templateType == "reset")
        {
            var copy = await _emailAppearanceService.GetCopyAsync(EmailTemplateCopy.Reset, null, token);
            html = ResetPasswordEmailTemplate.Build(look, copy, $"{frontendUrl}/account/reset-password?token=SAMPLE", 30, "محمد أحمد");
            subject = $"إعادة تعيين كلمة المرور | {look.BrandName}";
        }
        else
        {
            html = OrderEmailTemplate.Build(BuildSampleOrder(), frontendUrl, look);
            subject = $"تأكيد الطلب | {look.BrandName}";
        }

        try
        {
            await _emailService.SendEmail(new EmailDto(recipient, recipient, subject, html));
        }
        catch (Exception ex) when (ex is MailKit.Security.AuthenticationException or MailKit.ProtocolException
                                       or System.Net.Sockets.SocketException or TimeoutException
                                       or InvalidOperationException or System.Security.Cryptography.CryptographicException)
        {
            return BadRequest(new { message = "تعذر الإرسال. راجع إعدادات SMTP واحفظ كلمة المرور أولًا: " + ex.Message });
        }

        return Ok(new { message = $"تم إرسال النسخة التجريبية ({templateType}) إلى {recipient}." });
    }

    private async Task<EmailAppearanceOptions> LoadOwnerAppearanceAsync(CancellationToken token)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var settings = await _db.StoreSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerEmail == email, token);
        return EmailAppearanceOptions.From(settings);
    }

    private static object AppearanceResponse(EmailAppearanceOptions look) => new
    {
        accentColor = look.AccentColor,
        backgroundColor = look.BackgroundColor,
        paperColor = look.PaperColor,
        inkColor = look.InkColor,
        headingFont = look.HeadingFont,
        logoUrl = look.LogoUrl,
        headerStyle = look.HeaderStyle,
        brandName = look.BrandName,
        footerNote = look.FooterNote,
        eyebrowText = look.EyebrowText,
        greetingText = look.GreetingText,
        introText = look.IntroText,
        ctaText = look.CtaText
    };

    private static bool TryValidateAppearance(
        SaveEmailAppearanceRequest request,
        out string? accent, out string brand, out string? footerNote,
        out string? eyebrowText, out string? greetingText, out string? introText, out string? ctaText,
        out string? error)
    {
        accent = null; brand = string.Empty; footerNote = null;
        eyebrowText = null; greetingText = null; introText = null; ctaText = null;
        error = null;

        if (EmailAppearanceOptions.NormalizeColor(request.AccentColor) is null ||
            EmailAppearanceOptions.NormalizeColor(request.BackgroundColor) is null ||
            EmailAppearanceOptions.NormalizeColor(request.PaperColor) is null ||
            EmailAppearanceOptions.NormalizeColor(request.InkColor) is null)
        {
            error = "أحد الألوان غير صالح. استخدم صيغة hex مثل #2D5A43.";
            return false;
        }
        if (request.HeaderStyle is not ("minimal" or "accent"))
        {
            error = "نمط الهيدر غير معروف.";
            return false;
        }
        brand = request.BrandName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(brand) || brand.Length > 24)
        {
            error = "اسم البراند مطلوب بحد أقصى 24 حرفًا.";
            return false;
        }

        footerNote = TrimOrNull(request.FooterNote, 160);
        eyebrowText = TrimOrNull(request.EyebrowText, 40);
        greetingText = TrimOrNull(request.GreetingText, 80);
        introText = TrimOrNull(request.IntroText, 160);
        ctaText = TrimOrNull(request.CtaText, 40);
        if (request.FooterNote is { Length: > 160 } || eyebrowText is { Length: > 40 } ||
            greetingText is { Length: > 80 } || introText is { Length: > 160 } || ctaText is { Length: > 40 })
        {
            error = "أحد النصوص يتجاوز الحد الأقصى للطول.";
            return false;
        }

        var logo = request.LogoUrl?.Trim();
        if (!string.IsNullOrWhiteSpace(logo))
        {
            var isHttp = logo.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                         logo.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
            var isData = logo.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase);
            var isRel = logo.StartsWith("/", StringComparison.OrdinalIgnoreCase);
            if (!isHttp && !isData && !isRel)
            {
                error = "رابط اللوجو يجب أن يبدأ بـ https:// أو http:// أو يكون مسار صورة صالح.";
                return false;
            }
            if (!isData && logo.Length > 2000)
            {
                error = "رابط اللوجو طويل جدًا.";
                return false;
            }
        }

        accent = EmailAppearanceOptions.NormalizeColor(request.AccentColor);
        return true;
    }

    private static string? TrimOrNull(string? value, int max)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : (trimmed.Length > max ? trimmed[..max] : trimmed);
    }

    private static Order BuildSampleOrder() => new()
    {
        Id = 1024,
        OrderDate = DateTime.Now,
        PaymentMethod = "cod",
        Status = Status.Pending,
        SubTotal = 1590m,
        ShippingPrice = 0m,
        Discount = 0m,
        BuyerPhone = "01001234567",
        ShippingAddress = new ShippingAddress
        {
            FirstName = "محمد",
            LastName = "أحمد",
            Street = "14 شارع النصر",
            City = "المهندسين",
            State = "الجيزة"
        },
        DeliveryMethod = new DeliveryMethod
        {
            Name = "توصيل قياسي",
            DeliveryTime = "خلال 2 إلى 4 أيام عمل",
            Price = 0m
        },
        OrderItems = new List<OrderItem>
        {
            new() { ProductName = "كريم ترطيب بالشاي الأخضر", Price = 625m, Quantity = 2, MainImage = "" },
            new() { ProductName = "زيت الأرغان العضوي 50 مل", Price = 340m, Quantity = 1, MainImage = "" }
        }
    };
}
