using ECO.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace ECO.BLL.Services.Email;

/// <summary>
/// Reads email appearance (single StoreSettings row) and per-template copy
/// (EmailTemplateSetting rows) from the database for template building.
/// Saving happens through the admin API.
/// </summary>
public class EmailAppearanceService
{
    private readonly AppDbContext db;

    public EmailAppearanceService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<EmailAppearanceOptions> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await db.StoreSettings.AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        return EmailAppearanceOptions.From(settings);
    }

    /// <summary>Store-wide default email language ("ar" | "en").</summary>
    public async Task<string> GetDefaultLanguageAsync(CancellationToken cancellationToken = default)
    {
        var settings = await db.StoreSettings.AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        return settings?.EmailDefaultLanguage == "en" ? "en" : "ar";
    }

    /// <summary>Copy for a template type; uses the store default language when none given.</summary>
    public async Task<EmailTemplateCopy> GetCopyAsync(string templateType, string? language = null, CancellationToken cancellationToken = default)
    {
        var lang = EmailTemplateCopy.IsKnownLanguage(language)
            ? language!
            : await GetDefaultLanguageAsync(cancellationToken);

        var row = await db.EmailTemplateSettings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TemplateType == templateType && x.Language == lang, cancellationToken);

        return EmailTemplateCopy.Resolve(templateType, lang, row);
    }
}
