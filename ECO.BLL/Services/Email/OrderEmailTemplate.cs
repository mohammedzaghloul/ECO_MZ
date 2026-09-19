using System.Net;
using System.Text;
using ECO.DAL.Entities.OrderEntities;

namespace ECO.BLL.Services.Email;

public static class OrderEmailTemplate
{
    /// <summary>Per-language UI labels for the fixed parts of the template.</summary>
    private sealed record Labels(
        string OrderNoLabel, string OrderDateLabel, string PaymentLabel,
        string OrderNoPrefix, string ProductsTitle, string SubtotalLabel, string ShippingLabel,
        string DiscountLabel, string TotalLabel, string Currency, string QuantityPerUnit,
        string FreeLabel, string DeliveryTitle, string AddressTitle,
        string NeedHelp, string RightsSuffix,
        string[] Steps, string PaymentCod, string PaymentCard, string PaymentVodafone,
        string PaymentInsta, string FallbackAddress, string MissingItems)
    {
        public static readonly Labels Arabic = new(
            "رقم الطلب", "تاريخ الطلب", "طريقة الدفع",
            "رقم الطلب", "تفاصيل الطلب", "المجموع الفرعي", "الشحن والتوصيل",
            "الخصم", "الإجمالي", "ج.م", "الكمية {0} · {1} ج.م للقطعة",
            "مجاني", "🚚 التوصيل المتوقع", "عنوان التوصيل",
            "محتاج مساعدة؟", "جميع الحقوق محفوظة",
            new[] { "تم الاستلام", "قيد التجهيز", "تم الشحن", "تم التسليم" },
            "الدفع عند الاستلام", "بطاقة بنكية (أونلاين)", "فودافون كاش", "إنستاباي",
            "العنوان مسجل ومؤكد", "لا توجد تفاصيل منتجات مسجلة لهذا الطلب.");

        public static readonly Labels English = new(
            "Order No.", "Order Date", "Payment Method",
            "Order", "Order Details", "Subtotal", "Shipping",
            "Discount", "Total", "EGP", "Qty {0} · {1} EGP each",
            "Free", "🚚 Expected Delivery", "Delivery Address",
            "Need help?", "All rights reserved",
            new[] { "Placed", "Preparing", "Shipped", "Delivered" },
            "Cash on delivery", "Bank card (online)", "Vodafone Cash", "InstaPay",
            "Address on file", "No product details were recorded for this order.");
    }

    private static readonly string[] ArabicMonths =
    {
        "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
        "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
    };
    private static readonly string[] EnglishMonths =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    public static string Build(Order order, string frontendUrl, EmailAppearanceOptions? appearance = null, EmailTemplateCopy? copy = null)
    {
        var look = appearance ?? EmailAppearanceOptions.Default;
        var texts = copy ?? EmailTemplateCopy.Resolve(EmailTemplateCopy.Order, "ar", null);
        var isArabic = texts.Language != "en";
        var t = isArabic ? Labels.Arabic : Labels.English;

        var accent = look.AccentColor;
        var accentDeep = look.AccentDeep;
        var tint = look.Tint(0.08);
        var pageBg = look.BackgroundColor;
        var paper = look.PaperColor;
        var ink = look.InkColor;
        var inkSoft = look.InkSoft;
        var inkFaint = look.InkFaint;
        var line = look.LineColor;
        var serif = look.HeadingFontStack;
        var sans = EmailAppearanceOptions.BodyFontStack;
        var brand = WebUtility.HtmlEncode(look.BrandName);
        var footerNote = WebUtility.HtmlEncode(texts.FooterNote ?? (isArabic
            ? "فريق الدعم جاهز لمساعدتك عبر صفحة «تواصل معنا» في المتجر."
            : "Our support team is ready to help via the store's contact page."));

        var baseUrl = frontendUrl?.TrimEnd('/') ?? "http://localhost:4200";
        var firstNameRaw = order.ShippingAddress?.FirstName ?? (isArabic ? "عميلنا العزيز" : "dear customer");
        var customerName = WebUtility.HtmlEncode(firstNameRaw);
        var fullName = WebUtility.HtmlEncode($"{order.ShippingAddress?.FirstName} {order.ShippingAddress?.LastName}".Trim());
        if (string.IsNullOrWhiteSpace(fullName)) fullName = customerName;

        // Admin-editable copy ({name}/{order} placeholders), encoded after substitution.
        var eyebrow = EmailTemplateCopy.Fill(texts.EyebrowText, firstNameRaw, look.BrandName, order.Id);
        var greeting = EmailTemplateCopy.Fill(texts.GreetingText, firstNameRaw, look.BrandName, order.Id);
        var intro = EmailTemplateCopy.Fill(texts.IntroText, firstNameRaw, look.BrandName, order.Id);
        var ctaText = EmailTemplateCopy.Fill(texts.CtaText, firstNameRaw, look.BrandName, order.Id);

        var orderLink = $"{baseUrl}/my-order/{order.Id}";
        var orderDateStr = FormatDate(order.OrderDate, isArabic);

        var paymentMethodTitle = ResolvePaymentMethod(order.PaymentMethod, t);

        // Address resolution
        var addressList = new List<string>();
        if (!string.IsNullOrWhiteSpace(order.ShippingAddress?.Street)) addressList.Add(order.ShippingAddress.Street);
        if (!string.IsNullOrWhiteSpace(order.ShippingAddress?.City)) addressList.Add(order.ShippingAddress.City);
        if (!string.IsNullOrWhiteSpace(order.ShippingAddress?.State)) addressList.Add(order.ShippingAddress.State);
        var fullAddress = addressList.Count > 0 ? WebUtility.HtmlEncode(string.Join(isArabic ? "، " : ", ", addressList))
            : t.FallbackAddress;

        var phone = !string.IsNullOrWhiteSpace(order.BuyerPhone)
            ? WebUtility.HtmlEncode(order.BuyerPhone)
            : (!string.IsNullOrWhiteSpace(order.ShippingAddress?.ZipCode) ? WebUtility.HtmlEncode(order.ShippingAddress.ZipCode) : (isArabic ? "غير مسجل" : "Not provided"));

        var deliveryMethodName = WebUtility.HtmlEncode(order.DeliveryMethod?.Name ?? (isArabic ? "توصيل قياسي" : "Standard delivery"));
        var deliveryTime = !string.IsNullOrWhiteSpace(order.DeliveryMethod?.DeliveryTime)
            ? WebUtility.HtmlEncode(order.DeliveryMethod.DeliveryTime)
            : (isArabic ? "خلال 2 إلى 4 أيام عمل" : "Within 2-4 business days");

        // Build Order Items
        var itemsHtml = new StringBuilder();
        if (order.OrderItems != null && order.OrderItems.Count > 0)
        {
            foreach (var item in order.OrderItems)
            {
                var itemTotal = (item.Price * item.Quantity).ToString("N2");
                var unitPrice = item.Price.ToString("N2");
                var productName = WebUtility.HtmlEncode(item.ProductName ?? (isArabic ? "منتج" : "Product"));
                var imageUrl = ResolveImageUrl(item.MainImage, baseUrl);

                itemsHtml.Append($"""
                  <tr>
                    <td style="padding:14px 0; border-bottom:1px solid {line};">
                      <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                        <tr>
                          <td width="64" valign="middle" style="padding-left:16px;">
                            <div style="width:64px; height:64px; border-radius:4px; background-color:{tint}; border:1px solid {line}; overflow:hidden; text-align:center; line-height:64px;">
                              {(string.IsNullOrEmpty(imageUrl)
                                ? "&nbsp;"
                                : $"<img src=\"{WebUtility.HtmlEncode(imageUrl)}\" alt=\"{productName}\" width=\"64\" height=\"64\" style=\"width:64px; height:64px; object-fit:cover; display:block; border:0;\" onerror=\"this.style.display='none'\" />")}
                            </div>
                          </td>
                          <td valign="middle" style="text-align:right;">
                            <div style="font-size:15px; font-weight:600; color:{ink}; margin-bottom:4px;">{productName}</div>
                            <div style="font-size:13px; color:{inkSoft};">{string.Format(t.QuantityPerUnit, item.Quantity, unitPrice)}</div>
                          </td>
                          <td width="110" valign="middle" align="left" style="font-size:15px; font-weight:700; color:{ink}; white-space:nowrap;">
                            {itemTotal} {t.Currency}
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                """);
            }
        }
        else
        {
            itemsHtml.Append($"""
              <tr>
                <td style="padding:16px 0; text-align:center; color:{inkSoft}; font-size:14px;">
                  {t.MissingItems}
                </td>
              </tr>
            """);
        }

        // Financial rows
        var subTotalFormatted = order.SubTotal.ToString("N2");
        var shippingFormatted = order.ShippingPrice > 0
            ? $"{order.ShippingPrice:N2} {t.Currency}"
            : $"<span style=\"color:{accentDeep}; font-weight:600;\">{t.FreeLabel}</span>";

        var discountRow = "";
        if (order.Discount > 0)
        {
            discountRow = $"""
              <tr>
                <td style="padding:9px 0; border-bottom:1px solid {line}; font-size:14.5px; color:{inkSoft}; width:38%;">{t.DiscountLabel}</td>
                <td align="left" style="padding:9px 0; border-bottom:1px solid {line}; font-size:14.5px; font-weight:600; color:{accentDeep}; text-align:left;">- {order.Discount:N2} {t.Currency}</td>
              </tr>
            """;
        }

        var totalFormatted = order.GetTotal().ToString("N2");

        // Header band: minimal (paper) or accent (soft tinted).
        var headerBg = look.HeaderStyle == "accent" ? $"background-color:{look.Tint(0.07)};" : $"background-color:{paper};";
        var pageTitle = WebUtility.HtmlEncode(EmailTemplateCopy.Fill(texts.Subject, look.BrandName, firstNameRaw, order.Id));

        return $"""
        <!DOCTYPE html>
        <html lang="{(isArabic ? "ar" : "en")}" dir="{(isArabic ? "rtl" : "ltr")}">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>{pageTitle}</title>
          <link rel="preconnect" href="https://fonts.googleapis.com">
          <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
          <link href="{look.GoogleFontsHref}" rel="stylesheet">
        </head>
        <body style="margin:0; padding:0; background-color:{pageBg}; font-family:{sans}; direction:{(isArabic ? "rtl" : "ltr")}; text-align:{(isArabic ? "right" : "left")}; color:{ink}; -webkit-font-smoothing:antialiased;">

          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:{pageBg}; padding:24px 10px;">
            <tr>
              <td align="center">

                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:640px; background-color:{paper}; border:1px solid {line}; border-radius:2px; overflow:hidden;">

                  <!-- Header -->
                  <tr>
                    <td style="{headerBg} padding:30px 40px 0; text-align:center;">
                      {BuildBrandHeader(look, brand, serif, accentDeep)}
                      <div style="width:36px; height:1px; background-color:{accent}; margin:20px auto 0;"></div>
                    </td>
                  </tr>

                  <!-- Hero -->
                  <tr>
                    <td style="{headerBg} padding:22px 40px 26px; text-align:center;">
                      <div style="font-size:13px; color:{inkSoft}; margin-bottom:12px;">{eyebrow}</div>
                      <div style="width:30px; height:30px; border-radius:50%; background-color:{accent}; color:#fdfdfb; font-size:16px; font-weight:700; line-height:30px; text-align:center; margin:0 auto 14px;">✓</div>
                      <h1 style="font-family:{serif}; font-weight:600; font-size:26px; line-height:1.45; margin:0 0 10px; color:{ink};">{greeting}</h1>
                      <p style="font-size:15px; line-height:1.8; color:{inkSoft}; max-width:440px; margin:0 auto; padding:0 16px;">
                        {intro}
                      </p>
                      <div style="font-size:17px; font-weight:700; color:{accentDeep}; margin-top:18px; direction:ltr;">#{order.Id}</div>
                      <div style="font-size:12.5px; color:{inkSoft}; margin-top:5px;">{t.OrderNoPrefix} · {orderDateStr} · {paymentMethodTitle}</div>
                    </td>
                  </tr>

                  <!-- Order status stepper -->
                  <tr>
                    <td style="{headerBg} padding:0 26px 30px;">
                      {BuildStepper(order.Status, accent, tint, paper, line, ink, inkSoft, t.Steps)}
                    </td>
                  </tr>

                  <!-- Delivery estimate + CTA -->
                  <tr>
                    <td style="border-top:1px solid {line}; padding:22px 40px 26px; text-align:center;">
                      <div style="background-color:{tint}; border:1px solid {line}; border-radius:2px; padding:16px 18px;">
                        <div style="font-size:13.5px; font-weight:700; color:{ink};">{t.DeliveryTitle}</div>
                        <div style="font-size:16.5px; font-weight:700; color:{accentDeep}; margin-top:7px;">{deliveryTime}</div>
                        <div style="font-size:13px; color:{inkSoft}; margin-top:5px;">{deliveryMethodName}</div>
                      </div>
                      <a href="{WebUtility.HtmlEncode(orderLink)}" target="_blank" style="display:inline-block; background-color:{accent}; color:#fdfdfb; text-decoration:none; font-size:15.5px; font-weight:600; padding:16px 48px; border-radius:2px; letter-spacing:0.2px; text-align:center; margin-top:20px;">{ctaText}</a>
                    </td>
                  </tr>

                  <!-- Products -->
                  <tr>
                    <td style="border-top:1px solid {line}; padding:22px 40px;">
                      <div style="font-size:14px; font-weight:700; color:{ink}; margin-bottom:6px;">{t.ProductsTitle}</div>
                      <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                        {itemsHtml}
                      </table>
                    </td>
                  </tr>

                  <!-- Totals -->
                  <tr>
                    <td style="border-top:1px solid {line}; padding:20px 40px 24px;">
                      <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                        <tr>
                          <td style="padding:9px 0; border-bottom:1px solid {line}; font-size:14.5px; color:{inkSoft}; width:38%;">{t.SubtotalLabel}</td>
                          <td align="left" style="padding:9px 0; border-bottom:1px solid {line}; font-size:14.5px; font-weight:600; color:{ink}; text-align:left;">{subTotalFormatted} {t.Currency}</td>
                        </tr>
                        <tr>
                          <td style="padding:9px 0; border-bottom:1px solid {line}; font-size:14.5px; color:{inkSoft};">{t.ShippingLabel}</td>
                          <td align="left" style="padding:9px 0; border-bottom:1px solid {line}; font-size:14.5px; font-weight:600; color:{ink}; text-align:left;">{shippingFormatted}</td>
                        </tr>
                        {discountRow}
                        <tr>
                          <td style="border-top:2px solid {ink}; padding:16px 0 0; font-size:15.5px; font-weight:700; color:{ink};">{t.TotalLabel}</td>
                          <td align="left" style="border-top:2px solid {ink}; padding:16px 0 0; font-size:22px; font-weight:800; color:{accentDeep}; text-align:left;">{totalFormatted} <span style="font-size:13.5px; font-weight:600;">{t.Currency}</span></td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- Delivery address -->
                  <tr>
                    <td style="border-top:1px solid {line}; padding:20px 40px 24px;">
                      <div style="font-size:14px; font-weight:700; color:{ink}; margin-bottom:10px;">{t.AddressTitle}</div>
                      <div style="font-size:14.5px; font-weight:600; color:{ink};">{fullName}</div>
                      <div style="font-size:14px; color:{inkSoft}; line-height:1.8; margin-top:4px;">{fullAddress}</div>
                      <div style="font-size:14px; color:{inkSoft}; direction:ltr; text-align:{(isArabic ? "right" : "left")};">{phone}</div>
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td style="border-top:1px solid {line}; padding:22px 40px 26px; text-align:center;">
                      <div style="font-size:14px; font-weight:700; color:{ink}; margin-bottom:7px;">{t.NeedHelp}</div>
                      <p style="margin:0 0 8px; font-size:13px; color:{inkSoft}; line-height:1.8;">{footerNote}</p>
                      <p style="margin:0; font-size:11.5px; color:{inkFaint};">© {DateTime.Now.Year} {brand}. {t.RightsSuffix}.</p>
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

    /// <summary>Brand header: logo image when configured, otherwise the serif wordmark.</summary>
    private static string BuildBrandHeader(EmailAppearanceOptions look, string brand, string serif, string accentDeep)
    {
        if (string.IsNullOrWhiteSpace(look.LogoUrl))
            return $"""<div style="font-family:{serif}; font-size:22px; font-weight:600; letter-spacing:6px; color:{accentDeep};">{brand}</div>""";

        return $"""
          <img src="{WebUtility.HtmlEncode(look.LogoUrl)}" alt="{brand}" height="42" style="height:42px; max-width:220px; object-fit:contain; display:inline-block; border:0;" />
        """;
    }

    /// <summary>Four-step order timeline (email-safe: nested table, dots + 1px connectors).</summary>
    private static string BuildStepper(Status status, string accent, string tint, string paper, string line, string ink, string inkSoft, string[] labels)
    {
        var (doneCount, currentStep) = status switch
        {
            Status.Shipped => (2, 3),
            Status.Delivered => (4, 0),
            _ => (1, 2) // Pending / Payment states
        };

        var cells = new StringBuilder();
        for (var i = 1; i <= 4; i++)
        {
            var isDone = i <= doneCount;
            var isCurrent = i == currentStep;

            var dotBg = isDone ? accent : paper;
            var dotBorder = isDone || isCurrent ? accent : line;
            var dotShadow = isCurrent ? $"box-shadow:0 0 0 4px {tint};" : "";

            cells.Append($"""
              <td width="20%" align="center" valign="top" style="padding:0;">
                <div style="width:14px; height:14px; border-radius:50%; background-color:{dotBg}; border:2px solid {dotBorder}; {dotShadow} margin:0 auto;"></div>
              </td>
            """);

            if (i < 4)
            {
                var connectorColor = i <= doneCount ? accent : line;
                cells.Append($"""
                  <td width="6%" valign="top" style="padding:6px 0 0;">
                    <div style="height:2px; background-color:{connectorColor};"></div>
                  </td>
                """);
            }
        }

        var labelCells = new StringBuilder();
        for (var i = 1; i <= 4; i++)
        {
            var emphasized = i <= doneCount || i == currentStep;
            var color = emphasized ? ink : inkSoft;
            var weight = emphasized ? "font-weight:700; font-size:12.5px;" : "font-size:12px;";
            labelCells.Append($"""
              <td width="20%" align="center" style="padding:10px 0 0; color:{color}; {weight}">{labels[i - 1]}</td>
            """);
            if (i < 4)
                labelCells.Append($"""<td width="6%"></td>""");
        }

        return $"""
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
            <tr>{cells}</tr>
            <tr>{labelCells}</tr>
          </table>
        """;
    }

    private static string FormatDate(DateTime date, bool isArabic)
    {
        var hour12 = date.Hour % 12;
        if (hour12 == 0) hour12 = 12;
        if (isArabic)
        {
            var period = date.Hour < 12 ? "ص" : "م";
            return $"{date.Day} {ArabicMonths[date.Month - 1]} {date.Year} — {hour12}:{date.Minute:00} {period}";
        }
        var amPm = date.Hour < 12 ? "AM" : "PM";
        return $"{EnglishMonths[date.Month - 1]} {date.Day}, {date.Year} — {hour12}:{date.Minute:00} {amPm}";
    }

    private static string ResolvePaymentMethod(string? paymentMethod, Labels t)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod))
            return t.PaymentCod;

        return paymentMethod.Trim().ToLowerInvariant() switch
        {
            "stripe" or "card" or "creditcard" => t.PaymentCard,
            "vodafonecash" or "vodafone" => t.PaymentVodafone,
            "instapay" => t.PaymentInsta,
            "cod" or "cash" => t.PaymentCod,
            _ => WebUtility.HtmlEncode(paymentMethod)
        };
    }

    private static string ResolveImageUrl(string? imageUrl, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return string.Empty;

        var trimmed = imageUrl.Trim();
        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        if (trimmed.StartsWith("/"))
        {
            return $"{baseUrl}{trimmed}";
        }

        return $"{baseUrl}/{trimmed}";
    }
}
