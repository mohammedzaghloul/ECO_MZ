using System.Security.Claims;
using ECO.DAL.Data;
using ECO.DAL.Entities;
using ECO.BLL.DTO.Auth;
using ECO.BLL.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MailKit.Security;
using MailKit.Net.Smtp;
using ECO.BLL.DTO.StoreSettings;
using ECO.DAL.Entities.OrderEntities;

namespace ECO.Api.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StoreSettingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    public StoreSettingsController(AppDbContext db, IEmailService emailService, IConfiguration configuration)
    {
        _db = db;
        _emailService = emailService;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpGet("currency")]
    public async Task<IActionResult> GetCurrency(CancellationToken token)
    {
        var settings = await _db.StoreSettings.AsNoTracking().FirstOrDefaultAsync(token);
        return Ok(new { currencyCode = settings?.CurrencyCode ?? "EGP" });
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var settings = await _db.StoreSettings.AsNoTracking().FirstOrDefaultAsync(x => x.OwnerEmail == email, token);
        return Ok(new { currencyCode = settings?.CurrencyCode ?? "EGP" });
    }

    [HttpPut]
    public async Task<IActionResult> Save([FromBody] SaveStoreSettingsRequest request, CancellationToken token)
    {
        var code = request.CurrencyCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code) || code.Length > 8)
            return BadRequest(new { message = "Currency code is required." });

        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var settings = await _db.StoreSettings.FirstOrDefaultAsync(x => x.OwnerEmail == email, token);
        if (settings is null)
        {
            settings = new StoreSettings { OwnerEmail = email, CurrencyCode = code };
            await _db.StoreSettings.AddAsync(settings, token);
        }
        else
        {
            settings.CurrencyCode = code;
        }

        await _db.SaveChangesAsync(token);
        return Ok(new { currencyCode = code });
    }

    [HttpGet("email")]
    public async Task<IActionResult> GetEmailSettings(CancellationToken token)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var settings = await _db.StoreSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerEmail == email, token);
        return Ok(new
        {
            smtpHost = settings?.SmtpHost,
            smtpPort = settings?.SmtpPort,
            smtpUsername = settings?.SmtpUsername,
            smtpFrom = settings?.SmtpFrom,
            smtpUseSsl = settings?.SmtpUseSsl ?? true,
            passwordConfigured = !string.IsNullOrWhiteSpace(settings?.SmtpPasswordProtected)
        });
    }

    [HttpPut("email")]
    public async Task<IActionResult> SaveEmailSettings( [FromBody] SaveEmailSettingsRequest request, [FromServices] IDataProtectionProvider dataProtection, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.SmtpHost) ||
            request.SmtpPort is < 1 or > 65535 ||
            string.IsNullOrWhiteSpace(request.SmtpUsername) ||
            string.IsNullOrWhiteSpace(request.SmtpFrom))
            return BadRequest(new { message = "SMTP host, port, username, and sender are required." });

        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var settings = await _db.StoreSettings.FirstOrDefaultAsync(x => x.OwnerEmail == email, token);
        if (settings is null)
        {
            settings = new StoreSettings { OwnerEmail = email };
            await _db.StoreSettings.AddAsync(settings, token);
        }
        settings.SmtpHost = request.SmtpHost.Trim();
        settings.SmtpPort = request.SmtpPort;
        settings.SmtpUsername = request.SmtpUsername.Trim();
        settings.SmtpFrom = request.SmtpFrom.Trim();
        settings.SmtpUseSsl = request.SmtpUseSsl;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            // Google App Passwords are often copied with visual spaces.
            var smtpPassword = new string(request.Password.Where(c => !char.IsWhiteSpace(c)).ToArray());
            settings.SmtpPasswordProtected = dataProtection.CreateProtector("ECO.Settings.SmtpPassword")
                .Protect(smtpPassword);
        }
        await _db.SaveChangesAsync(token);
        return Ok(new { passwordConfigured = !string.IsNullOrWhiteSpace(settings.SmtpPasswordProtected) });
    }

    [HttpPost("email/test")]
    public async Task<IActionResult> TestEmail(CancellationToken token)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var settings = await _db.StoreSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerEmail == email, token);
        var recipient = settings?.SmtpFrom;
        if (settings is null || string.IsNullOrWhiteSpace(settings.SmtpHost) || settings.SmtpPort is < 1 or > 65535 ||  string.IsNullOrWhiteSpace(settings.SmtpUsername) ||
            string.IsNullOrWhiteSpace(recipient) ||
            string.IsNullOrWhiteSpace(settings.SmtpPasswordProtected))
            return BadRequest(new { message = "أكمل بيانات SMTP واحفظ كلمة المرور قبل إرسال رسالة الاختبار." });

        try
        {
            await _emailService.SendEmail(new EmailDto(
                recipient,
                recipient,
                $"رسالة اختبار | {EmailAppearanceOptions.From(settings).BrandName}",
                TestEmailTemplate.Build(EmailAppearanceOptions.From(settings))));
        }
        catch (AuthenticationException)
        {
            return BadRequest(new { message = "فشل تسجيل الدخول إلى SMTP. راجع اسم المستخدم وكلمة المرور أو استخدم App Password مع Gmail." });
        }
        catch (SmtpProtocolException)
        {
            return BadRequest(new { message = "رفض Gmail بيانات تسجيل الدخول. استخدم بريد Gmail الصحيح مع App Password مكوّن من 16 حرفًا، وليس كلمة مرور الحساب العادية." });
        }
        catch (SslHandshakeException)
        {
            return BadRequest(new { message = "فشل اتصال SSL. مع Gmail استخدم المنفذ 465 مع تفعيل SSL، أو المنفذ 587 مع إلغاء SSL." });
        }
        catch (TimeoutException)
        {
            return BadRequest(new { message = "انتهت مهلة الاتصال بخادم SMTP. راجع الخادم والمنفذ واتصال الإنترنت." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (SmtpCommandException)
        {
            return BadRequest(new { message = "رفض خادم SMTP الطلب. راجع إعدادات الحساب والمنفذ واسم المرسل." });
        }
        return Ok(new { message = "تم إرسال رسالة الاختبار إلى بريد المرسل." });
    }
}
