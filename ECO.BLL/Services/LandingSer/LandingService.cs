using ECO.BLL.DTO.LandingDtos;
using ECO.DAL.Entities.Landing;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Interfaces;
using ECO.DAL.Specifications;
using ECO.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECO.BLL.Services.LandingSer
{
    public class LandingService : ILandingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDbContext _dbContext;

        public LandingService(IUnitOfWork unitOfWork, AppDbContext dbContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<LandingPageSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var pages = await _unitOfWork.Repository<LandingPage>()
                .ListAsync(new LandingPageSpecification(), cancellationToken);

            var pageIds = pages.Select(p => p.Id).ToHashSet();

            // Aggregate order count per landing page
            var orderCounts = await _dbContext.Orders
                .AsNoTracking()
                .Where(order => order.LandingPageId.HasValue && pageIds.Contains(order.LandingPageId.Value))
                .GroupBy(order => order.LandingPageId!.Value)
                .Select(group => new { LandingPageId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.LandingPageId, item => item.Count, cancellationToken);
            var visitCounts = await _dbContext.LandingPageEvents
                .AsNoTracking()
                .Where(item => item.EventType == "visit" && pageIds.Contains(item.LandingPageId))
                .GroupBy(item => item.LandingPageId)
                .Select(group => new
                {
                    LandingPageId = group.Key,
                    Count = group.Select(item => item.SessionId).Distinct().Count()
                })
                .ToDictionaryAsync(item => item.LandingPageId, item => item.Count, cancellationToken);

            return pages
                .Select(page =>
                {
                    var ordersCount = orderCounts.TryGetValue(page.Id, out var count) ? count : 0;
                    var views = visitCounts.TryGetValue(page.Id, out var visitCount) ? visitCount : 0;
                    var conversionRate = views > 0 
                        ? Math.Round((decimal)ordersCount * 100m / views, 2) 
                        : 0;

                    return new LandingPageSummaryDto
                    {
                        Id = page.Id,
                        Title = page.Title,
                        Slug = page.Slug,
                        ProductId = page.ProductId,
                        ProductName = page.Product?.Name,
                        IsPublished = page.IsPublished,
                        ViewCount = views,
                        OrdersCount = ordersCount,
                        ConversionRate = conversionRate,
                        SectionsCount = page.Sections.Count,
                        UpdatedAt = page.UpdatedAt
                    };
                })
                .ToList();
        }

        public async Task<LandingPageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var page = await _unitOfWork.Repository<LandingPage>()
                .FirstOrDefaultAsync(new LandingPageSpecification(id), cancellationToken);
            return page is null ? null : MapToDto(page);
        }

        public async Task<LandingPageDto?> GetBySlugAsync(string slug, bool countView = false, CancellationToken cancellationToken = default)
        {
            var page = await _unitOfWork.Repository<LandingPage>()
                .FirstOrDefaultAsync(new LandingPageSpecification(slug), cancellationToken);

            if (page is null)
                return null;

            if (countView)
            {
                page.ViewCount++;
                await _unitOfWork.Repository<LandingPage>().UpdateAsync(page, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
            }

            return MapToDto(page);
        }

        public async Task<LandingPageDto> SaveAsync(SaveLandingPageDto dto, CancellationToken cancellationToken = default)
        {
            var slugConflict = await _unitOfWork.Repository<LandingPage>().FirstOrDefaultAsync(
                new LandingPageSpecification(dto.Slug, dto.Id), cancellationToken);
            if (slugConflict is not null)
                throw new InvalidOperationException($"The slug '{dto.Slug}' is already used by another landing page.");

            LandingPage page;
            if (dto.Id > 0)
            {
                page = await _unitOfWork.Repository<LandingPage>()
                    .FirstOrDefaultAsync(new LandingPageSpecification(dto.Id), cancellationToken);
                if (page is null)
                    throw new InvalidOperationException($"Landing page with id {dto.Id} was not found.");

                page.Title = dto.Title;
                page.Slug = dto.Slug;
                page.ProductId = dto.ProductId;
                page.Template = dto.Template;
                page.AccentColor = dto.AccentColor;
                page.FontFamily = dto.FontFamily;
                page.VideoUrl = dto.VideoUrl;
                page.WhatsAppNumber = dto.WhatsAppNumber;
                page.WhatsAppMessage = dto.WhatsAppMessage;
                page.IsPublished = dto.IsPublished;
                page.UpdatedAt = DateTime.UtcNow;

                // Replace-all strategy: sections are rewritten on every save.
                foreach (var existingSection in page.Sections.ToList())
                {
                    await _unitOfWork.Repository<LandingPageSection>()
                        .DeleteAsync(existingSection.Id, cancellationToken);
                }
            }
            else
            {
                page = new LandingPage
                {
                    Title = dto.Title,
                    Slug = dto.Slug,
                    ProductId = dto.ProductId,
                    Template = dto.Template,
                    AccentColor = dto.AccentColor,
                    FontFamily = dto.FontFamily,
                    VideoUrl = dto.VideoUrl,
                    WhatsAppNumber = dto.WhatsAppNumber,
                    WhatsAppMessage = dto.WhatsAppMessage,
                    IsPublished = dto.IsPublished,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<LandingPage>().AddAsync(page, cancellationToken);
                // Save immediately so page.Id is generated before sections reference it.
                await _unitOfWork.CompleteAsync(cancellationToken);
            }

            var sortOrder = 0;
            foreach (var sectionDto in dto.Sections)
            {
                await _unitOfWork.Repository<LandingPageSection>().AddAsync(new LandingPageSection
                {
                    LandingPageId = page.Id,
                    SectionType = sectionDto.SectionType,
                    Title = sectionDto.Title ?? string.Empty,
                    SortOrder = sortOrder++,
                    IsVisible = sectionDto.IsVisible,
                    ImageUrl = sectionDto.ImageUrl ?? string.Empty,
                    ContentJson = string.IsNullOrWhiteSpace(sectionDto.ContentJson) ? "{}" : sectionDto.ContentJson
                }, cancellationToken);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            var saved = await _unitOfWork.Repository<LandingPage>()
                .FirstOrDefaultAsync(new LandingPageSpecification(page.Id), cancellationToken);

            return MapToDto(saved!);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var page = await _unitOfWork.Repository<LandingPage>().GetByIdAsync(id, cancellationToken);
            if (page is null)
                return false;

            await _unitOfWork.Repository<LandingPage>().DeleteAsync(id, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            return true;
        }

        public async Task<LandingPageDto> GenerateDefaultForProductAsync(GenerateDefaultForProductDto generateDefaultForProduct, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.Repository<LandingPage>()
                .FirstOrDefaultAsync(LandingPageSpecification.ForProduct(generateDefaultForProduct.productId), cancellationToken);
            if (existing != null)
            {
                return await GetByIdAsync(existing.Id, cancellationToken) ?? MapToDto(existing);
            }

            var baseSlug = GenerateSlug(generateDefaultForProduct.productName);
            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = $"product-{generateDefaultForProduct.productId}";

            var slug = baseSlug;
            var counter = 1;
            while (await _unitOfWork.Repository<LandingPage>()
                .FirstOrDefaultAsync(new LandingPageSpecification(slug), cancellationToken) != null)
            {
                slug = $"{baseSlug}-{counter++}";
            }

            var cleanDesc = !string.IsNullOrWhiteSpace(generateDefaultForProduct.description)
                ? generateDefaultForProduct.description
                : "تصميم أنيق وبسيط يعكس الجودة العالية والاهتمام بأدق التفاصيل.";

            var heroJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                headline = generateDefaultForProduct.productName,
                body = cleanDesc,
                buttonText = "اطلب الآن مباشرة",
                buttonLink = "#orderform"
            });

            var featuresJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                headline = "لماذا يناسبك هذا المنتج؟",
                items = new[]
                {
                    new { title = "خامات وتصنيع فائق الدقة", text = "تم انتقاء المواد بعناية فائقة لضمان المتانة والراحة.", icon = "verified" },
                    new { title = "شحن سريع ومعاينة قبل الاستلام", text = "توصيل آمن إلى باب منزلك مع حق الفحص الكامل قبل الدفع.", icon = "local_shipping" },
                    new { title = "استبدال واسترجاع ميسر", text = "خدمة ما بعد البيع متميزة لدعمك والإجابة عن أي استفسار.", icon = "autorenew" }
                }
            });

            var trustbarJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                items = new[]
                {
                    new { icon = "local_shipping", title = "شحن سريع لكل المحافظات" },
                    new { icon = "payments", title = "الدفع عند الاستلام" },
                    new { icon = "sync", title = "استبدال خلال 14 يوم" },
                    new { icon = "support_agent", title = "دعم عملاء طوال الأسبوع" }
                }
            });

            // Real customer reviews for this product — the section is skipped
            // entirely when the product has none (no fabricated reviews).
            var productReviews = await _unitOfWork.Repository<ECO.DAL.Entities.Product.Review>()
                .ListAsync(new ECO.DAL.Specifications.ReviewSpecification(generateDefaultForProduct.productId), cancellationToken);

            var reviewItems = productReviews
                .Select((review, index) => new
                {
                    name = review.User?.DisplayName ?? review.User?.UserName ?? $"عميل ECO #{index + 1}",
                    rating = review.Rating,
                    text = review.Comment
                })
                .ToList();

            string? reviewsJson = reviewItems.Count > 0
                ? System.Text.Json.JsonSerializer.Serialize(new { headline = "آراء عملائنا", items = reviewItems })
                : null;

            // Default FAQ answers the purchase objections customers actually ask about.
            var faqJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                headline = "أسئلة قبل الطلب",
                items = new[]
                {
                    new { question = "هل يوجد دفع عند الاستلام؟", answer = "نعم، تدفع كاش للمندوب بعد فحص ومعاينة المنتج بنفسك." },
                    new { question = "التوصيل بياخد قد إيه؟", answer = "من 2 إلى 5 أيام عمل حسب المحافظة، وهنتواصل معك هاتفياً لتأكيد الطلب قبل الشحن." },
                    new { question = "هل يوجد ضمان على المنتج؟", answer = "نعم، المنتج مضمون ضد عيوب الصناعة، وفي حالة أي عيب مصنعي هيتم استبداله." },
                    new { question = "هل يمكن الاستبدال أو الاسترجاع؟", answer = "نعم، يحق لك الاستبدال أو الاسترجاع خلال 14 يوم من الاستلام بنفس حالة المنتج." },
                    new { question = "ماذا لو وصل المنتج تالفًا أو فيه مشكلة؟", answer = "تواصل معنا فورًا وهنستبدله لك بالكامل على حسابنا بدون أي تكلفة إضافية." }
                }
            });

            var orderFormJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                headline = "طلب مباشر وسريع",
                body = "سجل بيانات التوصيل والدفع عند الاستلام بعد المعاينة"
            });

            var landingPage = new LandingPage
            {
                Title = generateDefaultForProduct.productName,
                Slug = slug,
                ProductId = generateDefaultForProduct.productId,
                Template = "minimal",
                AccentColor = "#2d5a43", // Calm Korean Sage / Earth Green Accent
                FontFamily = "system",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<LandingPage>().AddAsync(landingPage, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            var sections = new List<LandingPageSection>
            {
                new()
                {
                    LandingPageId = landingPage.Id,
                    SectionType = "hero",
                    Title = "نظرة عامة",
                    SortOrder = 0,
                    IsVisible = true,
                    ImageUrl = generateDefaultForProduct.mainImageUrl ?? string.Empty,
                    ContentJson = heroJson
                },
                new()
                {
                    LandingPageId = landingPage.Id,
                    SectionType = "trustbar",
                    Title = "ضماناتنا",
                    SortOrder = 1,
                    IsVisible = true,
                    ImageUrl = string.Empty,
                    ContentJson = trustbarJson
                },
                new()
                {
                    LandingPageId = landingPage.Id,
                    SectionType = "features",
                    Title = "المميزات",
                    SortOrder = 2,
                    IsVisible = true,
                    ImageUrl = string.Empty,
                    ContentJson = featuresJson
                }
            };

            if (reviewsJson != null)
            {
                sections.Add(new LandingPageSection
                {
                    LandingPageId = landingPage.Id,
                    SectionType = "reviews",
                    Title = "آراء العملاء",
                    SortOrder = 3,
                    IsVisible = true,
                    ImageUrl = string.Empty,
                    ContentJson = reviewsJson
                });
            }

            sections.Add(new LandingPageSection
            {
                LandingPageId = landingPage.Id,
                SectionType = "faq",
                Title = "أسئلة شائعة",
                SortOrder = 4,
                IsVisible = true,
                ImageUrl = string.Empty,
                ContentJson = faqJson
            });

            // The order form stays last: the customer sees benefits, guarantees,
            // reviews and FAQ before being asked for delivery details.
            sections.Add(new LandingPageSection
            {
                LandingPageId = landingPage.Id,
                SectionType = "orderform",
                Title = "اطلب الآن",
                SortOrder = 5,
                IsVisible = true,
                ImageUrl = string.Empty,
                ContentJson = orderFormJson
            });

            foreach (var section in sections)
            {
                await _unitOfWork.Repository<LandingPageSection>().AddAsync(section, cancellationToken);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);

            var saved = await _unitOfWork.Repository<LandingPage>()
                .FirstOrDefaultAsync(new LandingPageSpecification(landingPage.Id), cancellationToken);

            return MapToDto(saved ?? landingPage);
        }

        private static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = text.Trim().ToLowerInvariant();
            var sb = new StringBuilder();
            var lastWasHyphen = false;

            foreach (var ch in normalized)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                    lastWasHyphen = false;
                }
                else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_')
                {
                    if (!lastWasHyphen && sb.Length > 0)
                    {
                        sb.Append('-');
                        lastWasHyphen = true;
                    }
                }
            }

            return sb.ToString().Trim('-');
        }

        private static LandingPageDto MapToDto(LandingPage page)
        {
            return new LandingPageDto
            {
                Id = page.Id,
                Title = page.Title,
                Slug = page.Slug,
                ProductId = page.ProductId,
                Template = page.Template,
                AccentColor = page.AccentColor,
                FontFamily = page.FontFamily,
                VideoUrl = page.VideoUrl,
                WhatsAppNumber = page.WhatsAppNumber,
                WhatsAppMessage = page.WhatsAppMessage,
                IsPublished = page.IsPublished,
                ViewCount = page.ViewCount,
                CreatedAt = page.CreatedAt,
                UpdatedAt = page.UpdatedAt,
                Sections = page.Sections
                    .OrderBy(section => section.SortOrder)
                    .Select(section => new LandingSectionDto
                    {
                        Id = section.Id,
                        SectionType = section.SectionType,
                        Title = section.Title,
                        SortOrder = section.SortOrder,
                        IsVisible = section.IsVisible,
                        ImageUrl = section.ImageUrl,
                        ContentJson = section.ContentJson
                    })
                    .ToList()
            };
        }
    }
}
