using ECO.DAL.Entities;

namespace ECO.BLL.Services.Email;

/// <summary>
/// Resolved copy for one template type + language, merged from the
/// EmailTemplateSetting row (admin-edited) over built-in defaults.
/// Placeholders available: {name} {order} {brand}.
/// </summary>
public sealed record EmailTemplateCopy(
    string TemplateType,
    string Language,
    string Subject,
    string EyebrowText,
    string GreetingText,
    string IntroText,
    string CtaText,
    string? FooterNote)
{
    public const string Order = "order";
    public const string Reset = "reset";
    public const string Activation = "activation";

    public static bool IsKnownType(string? type) => type is Order or Reset or Activation;
    public static bool IsKnownLanguage(string? lang) => lang is "ar" or "en";

    private static EmailTemplateCopy DefaultFor(string type, string lang) => (type, lang) switch
    {
        (Order, "en") => new EmailTemplateCopy(type, lang,
            "Order Confirmation #{order} | {brand}",
            "Order Confirmation",
            "Your order is confirmed",
            "Thank you, {name}! We've received your order and it's being prepared.",
            "Track your order",
            "Questions about your order? Reach us anytime via the store's contact page."),
        (Order, _) => new EmailTemplateCopy(type, "ar",
            "تأكيد الطلب #{order} | {brand}",
            "تأكيد الطلب",
            "تم تأكيد طلبك بنجاح",
            "شكرًا لك، {name}! استلمنا طلبك وهو الآن قيد التجهيز.",
            "تتبع طلبك",
            "فريق الدعم جاهز لمساعدتك عبر صفحة «تواصل معنا» في المتجر."),
        (Reset, "en") => new EmailTemplateCopy(type, lang,
            "Password Reset | {brand}",
            "Password Reset",
            "Password reset requested",
            "We received a request to reset the password for your account. Click the button below to choose a new one.",
            "Set a new password",
            "If you didn't request this, ignore this email — your account is safe."),
        (Reset, _) => new EmailTemplateCopy(type, "ar",
            "إعادة تعيين كلمة المرور | {brand}",
            "إعادة تعيين كلمة المرور",
            "وصل طلب إعادة تعيين كلمة المرور",
            "استلمنا طلب تعيين كلمة مرور جديدة لحسابك. اضغط الزر التالي لاختيار كلمة مرور جديدة.",
            "تعيين كلمة مرور جديدة",
            "إن لم تطلب إعادة التعيين، تجاهل هذه الرسالة وحسابك في أمان."),
        (Activation, "en") => new EmailTemplateCopy(type, lang,
            "Account Activation | {brand}",
            "Account Activation",
            "Your account awaits activation",
            "One last step and your account is ready. Click the button below to confirm your email and start shopping.",
            "Activate account",
            "If you didn't create an account with us, ignore this email."),
        _ => new EmailTemplateCopy(type, "ar",
            "تفعيل الحساب | {brand}",
            "تفعيل الحساب",
            "حسابك بانتظار التفعيل",
            "خطوة أخيرة ويصبح حسابك جاهز. اضغط الزر التالي لتفعيل بريدك والبدء في التسوق.",
            "تفعيل الحساب",
            "إن لم تنشئ حسابًا لدينا، تجاهل هذه الرسالة.")
    };

    /// <summary>Merges an optional admin-edited row over the built-in defaults.</summary>
    public static EmailTemplateCopy Resolve(string type, string language, EmailTemplateSetting? row)
    {
        var defaults = DefaultFor(type, language);
        if (row is null)
            return defaults;

        return new EmailTemplateCopy(
            type,
            language,
            string.IsNullOrWhiteSpace(row.Subject) ? defaults.Subject : row.Subject!.Trim(),
            string.IsNullOrWhiteSpace(row.EyebrowText) ? defaults.EyebrowText : row.EyebrowText!.Trim(),
            string.IsNullOrWhiteSpace(row.GreetingText) ? defaults.GreetingText : row.GreetingText!.Trim(),
            string.IsNullOrWhiteSpace(row.IntroText) ? defaults.IntroText : row.IntroText!.Trim(),
            string.IsNullOrWhiteSpace(row.CtaText) ? defaults.CtaText : row.CtaText!.Trim(),
            string.IsNullOrWhiteSpace(row.FooterNote) ? null : row.FooterNote!.Trim());
    }

    /// <summary>Fills {name}/{order}/{brand} in template text and HTML-encodes it.</summary>
    public static string Fill(string? template, string name, string brand, int? orderId)
    {
        if (string.IsNullOrWhiteSpace(template)) return string.Empty;
        var filled = template
            .Replace("{brand}", brand ?? string.Empty)
            .Replace("{name}", name ?? string.Empty)
            .Replace("{order}", orderId?.ToString() ?? string.Empty);
        return System.Net.WebUtility.HtmlEncode(filled);
    }

    /// <summary>Fills {name}/{order}/{brand} in a subject line and HTML-encodes it.</summary>
    public static string FillSubject(string subject, string brand, string? customerName, int? orderId)
    {
        var filled = subject
            .Replace("{brand}", brand)
            .Replace("{name}", customerName ?? string.Empty)
            .Replace("{order}", orderId?.ToString() ?? string.Empty);
        return System.Net.WebUtility.HtmlEncode(filled);
    }
}
