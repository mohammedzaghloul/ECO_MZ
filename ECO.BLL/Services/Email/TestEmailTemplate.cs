namespace ECO.BLL.Services.Email;

/// <summary>Minimal test email in the store's configured appearance (warm paper style).</summary>
public static class TestEmailTemplate
{
    public static string Build(EmailAppearanceOptions look)
    {
        var accent = look.AccentColor;
        var accentDeep = look.AccentDeep;
        var pageBg = look.BackgroundColor;
        var paper = look.PaperColor;
        var ink = look.InkColor;
        var inkSoft = look.InkSoft;
        var inkFaint = look.InkFaint;
        var line = look.LineColor;
        var serif = look.HeadingFontStack;
        var brand = System.Net.WebUtility.HtmlEncode(look.BrandName);

        var headerBg = look.HeaderStyle == "accent" ? $"background-color:{look.Tint(0.07)};" : $"background-color:{paper};";

        return $"""
        <!DOCTYPE html>
        <html lang="ar" dir="rtl">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>رسالة اختبار - {brand}</title>
          <link rel="preconnect" href="https://fonts.googleapis.com">
          <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
          <link href="{look.GoogleFontsHref}" rel="stylesheet">
        </head>
        <body style="margin:0; padding:0; background-color:{pageBg}; font-family:{EmailAppearanceOptions.BodyFontStack}; direction:rtl; text-align:right; color:{ink};">

          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:{pageBg}; padding:28px 10px;">
            <tr>
              <td align="center">

                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:640px; background-color:{paper}; border:1px solid {line}; border-radius:2px; overflow:hidden;">

                  <tr>
                    <td style="{headerBg} padding:38px 40px 34px; text-align:center;">
                      <div style="font-family:{serif}; font-size:21px; font-weight:600; letter-spacing:6px; color:{accentDeep};">{brand}</div>
                      <div style="width:36px; height:1px; background-color:{accent}; margin:24px auto;"></div>
                      <div style="font-size:13px; color:{inkSoft}; margin-bottom:10px;">رسالة اختبار</div>
                      <h1 style="font-family:{serif}; font-weight:600; font-size:22px; line-height:1.5; margin:0 0 12px; color:{ink};">إعدادات البريد تعمل بشكل صحيح</h1>
                      <p style="font-size:14px; line-height:1.9; color:{inkSoft}; max-width:420px; margin:0 auto; padding:0 20px;">
                        تم إرسال هذه الرسالة عبر خادم SMTP المحفوظ في لوحة تحكم المتجر، ولم تظهر أي أخطاء أثناء الإرسال.
                      </p>
                    </td>
                  </tr>

                  <tr>
                    <td style="border-top:1px solid {line}; padding:24px 40px 28px; text-align:center;">
                      <p style="margin:0; font-size:11.5px; color:{inkFaint};">© {DateTime.Now.Year} {brand}. جميع الحقوق محفوظة.</p>
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
