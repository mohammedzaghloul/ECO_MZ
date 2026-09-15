using ECO.DAL.Interfaces;
using ECO.DAL.Data;
using ECO.DAL.Repostories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECO.DAL
{
    public static class InfrastractionRegsitertion
    {
        public static IServiceCollection AddInfrastractionConfiguration(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddDbContext<AppDbContext>(options =>
                     options.UseSqlServer(configuration.GetConnectionString("EcoDataBase")));
            
            
            return services;
        }

    }
}
