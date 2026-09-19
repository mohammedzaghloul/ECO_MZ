using ECO.BLL.AutoMapper;
using ECO.BLL.Services;
using ECO.BLL.Services.AdminSer;
using ECO.BLL.Services.Basket;
using ECO.BLL.Services.CategorySer;
using ECO.BLL.Services.CouponSer;
using ECO.BLL.Services.Email;
using ECO.BLL.Services.Identity;
using ECO.BLL.Services.LandingSer;
using ECO.BLL.Services.Orderserv;
using ECO.BLL.Services.Payment;
using ECO.BLL.Services.ProductServices;
using ECO.BLL.Services.ReviewSer;
using ECO.BLL.Services.Token;
using ECO.BLL.Services.Upload;
using ECO.BLL.Services.UserInfo;
using ECO.BLL.Services.WishlistSer;
using ECO.BLL.Services.Notifications;
using ECO.DAL.Entities.Product;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

namespace ECO.BLL
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IImageManagementService, ImageManagementService>();
            services.AddAutoMapper(a => a.AddProfile<MappingProfile>());
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
            services.AddScoped<IBasketService,BasketService>();
            services.AddScoped<IEmailService,EmailService>();
            services.AddScoped<EmailAppearanceService>();
            services.AddScoped<IAuthService,AuthService>();
            services.AddScoped<IGenerateToken, GenerateToken>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IOrderService,OrderService>();
            services.AddScoped<IGuestOrderService, GuestOrderService>();
            services.AddOptions<BasketSettings>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IWishlistService, WishlistService>();
            services.AddScoped<ILandingService, LandingService>();
            services.AddScoped<ILandingAnalyticsService, LandingAnalyticsService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAdminOrderService, AdminOrderService>();
            services.AddScoped<ICustomerAdminService, CustomerAdminService>();
            services.AddScoped<IAdminDiscountService, AdminDiscountService>();
            services.AddScoped<ILocationCatalogService, LocationCatalogService>();
            services.AddScoped<LocationService>();
            services.AddScoped<ICouponService, CouponService>();

            var dataProtectionKeysPath = Path.Combine(AppContext.BaseDirectory, "dataprotection-keys");
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

            return services;
        }
    }
}
