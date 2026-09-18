using ECO.BLL.AutoMapper;
using ECO.BLL.Services.CategorySer;
using ECO.BLL.Services.ProductServices;
using ECO.BLL.Services.Upload;
using ECO.DAL.Entites.Product;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace ECO.BLL
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IImageManagementService, ImageManagementService>();
            services.AddAutoMapper(a => a.AddProfile<MappingProfile>());
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
            return services;
        }
    }
}
