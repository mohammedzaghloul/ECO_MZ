using System.Net;

namespace ECO.BLL.Services.Email;

/// <summary>
/// Account activation (email confirmation) email. Same paper style as
/// OrderEmailTemplate, driven by EmailAppearanceOptions (theme) + EmailTemplateCopy
/// (per-type/language copy). The activation link carries the Identity token.
/// </summary>
public static class AccountActivationEmailTemplate
{
    public static string Build(EmailAppearanceOptions look, EmailTemplateCopy copy, string activationLink, int expiryMinutes, string? displayName = null)
    {
        var isArabic = copy.Language != "en";
        var accent = look.AccentColor;
        var accentDeep = look.AccentDeep;
        var pageBg = look.BackgroundColor;
        var paper = look.PaperColor;
        var ink = look.InkColor;
        var inkSoft = look.InkSoft;
        var inkFaint = look.InkFaint;
        var line = look.LineColor;
        var serif = look.HeadingFontStack;
        var brand = WebUtility.HtmlEncode(look.BrandName);
        var brandHeader = string.IsNullOrWhiteSpace(look.LogoUrl)
            ? $"""<div style="font-family:{serif}; font-size:21px; font-weight:600; letter-spacing:6px; color:{accentDeep};">{brand}</div>"""
            : $"""<img src="{WebUtility.HtmlEncode(look.LogoUrl)}" alt="{brand}" height="42" style="height:42px; max-width:220px; object-fit:contain; display:inline-block; border:0;" />""";
        var name = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(displayName) ? look.BrandName : displayName!.Trim());
        var encodedLink = WebUtility.HtmlEncode(activationLink);
        var eyebrow = EmailTemplateCopy.Fill(copy.EyebrowText, displayName ?? string.Empty, look.BrandName, null);
        var greeting = EmailTemplateCopy.Fill(copy.GreetingText, displayName ?? string.Empty, look.BrandName, null);
        var intro = EmailTemplateCopy.Fill(copy.IntroText, displayName ?? string.Empty, look.BrandName, null);
        var ctaText = EmailTemplateCopy.Fill(copy.CtaText, displayName ?? string.Empty, look.BrandName, null);
        var footerNote = WebUtility.HtmlEncode(copy.FooterNote ?? (isArabic
            ? "إن لم تنشئ حسابًا لدينا، تجاهل هذه الرسالة."
            : "If you didn't create an account with us, ignore this email."));

        var altText = isArabic
            ? "إذا لم يعمل الزر، انسخ الرابط التالي والصقه في المتصفح:"
            : "If the button doesn't work, copy and paste this link into your browser:";
        var expiryText = isArabic
            ? $"هذا الرابط صالح لمدة {expiryMinutes} دقيقة."
            : $"This link is valid for {expiryMinutes} minutes.";

        var headerBg = look.HeaderStyle == "accent" ? $"background-color:{look.Tint(0.07)};" : $"background-color:{paper};";

        return $"""
        <!DOCTYPE html>
        <html lang="{(isArabic ? "ar" : "en")}" dir="{(isArabic ? "rtl" : "ltr")}">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>{WebUtility.HtmlEncode(EmailTemplateCopy.Fill(copy.Subject, look.BrandName, displayName, null))}</title>
          <link rel="preconnect" href="https://fonts.googleapis.com">
          <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
          <link href="{look.GoogleFontsHref}" rel="stylesheet">
        </head>
        <body style="margin:0; padding:0; background-color:{pageBg}; font-family:{EmailAppearanceOptions.BodyFontStack}; direction:{(isArabic ? "rtl" : "ltr")}; text-align:{(isArabic ? "right" : "left")}; color:{ink}; -webkit-font-smoothing:antialiased;">

          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:{pageBg}; padding:28px 10px;">
            <tr>
              <td align="center">

                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:640px; background-color:{paper}; border:1px solid {line}; border-radius:2px; overflow:hidden;">

                  <tr>
                    <td style="{headerBg} padding:40px 40px 34px; text-align:center;">
                      {brandHeader}
                      <div style="width:36px; height:1px; background-color:{accent}; margin:24px auto;"></div>
                      <div style="font-size:13px; color:{inkSoft}; margin-bottom:10px;">{eyebrow}</div>
                      <h1 style="font-family:{serif}; font-weight:600; font-size:23px; line-height:1.5; margin:0 0 12px; color:{ink};">{greeting}</h1>
                      <p style="font-size:14.5px; line-height:1.9; color:{inkSoft}; max-width:440px; margin:0 auto; padding:0 20px;">
                        {intro}
                      </p>
                    </td>
                  </tr>

                  <tr>
                    <td style="border-top:1px solid {line}; padding:34px 40px; text-align:center;">
                      <a href="{encodedLink}" target="_blank" style="display:inline-block; background-color:{accent}; color:#fdfdfb; text-decoration:none; font-size:15.5px; font-weight:600; padding:16px 44px; border-radius:2px; letter-spacing:0.2px; text-align:center;">{ctaText}</a>
                      <p style="font-size:12.5px; color:{inkSoft}; line-height:1.8; margin:22px auto 0; max-width:420px;">
                        {expiryText}
                      </p>
                    </td>
                  </tr>

                  <tr>
                    <td style="border-top:1px solid {line}; padding:24px 40px 28px; text-align:center;">
                      <p style="font-size:12px; color:{inkSoft}; margin:0 0 8px; line-height:1.8;">{altText}</p>
                      <p style="font-size:11.5px; color:{inkFaint}; direction:ltr; margin:0; word-break:break-all;">{encodedLink}</p>
                    </td>
                  </tr>

                  <tr>
                    <td style="border-top:1px solid {line}; padding:22px 40px 26px; text-align:center;">
                      <p style="margin:0; font-size:11.5px; color:{inkFaint};">© {DateTime.Now.Year} {brand}. {(isArabic ? "جميع الحقوق محفوظة." : "All rights reserved.")}</p>
                    </td>
                  </tr>

                </table>

              </td>
            </tr>
          </table>

        </body>
        </html>
        """;
    }
}
