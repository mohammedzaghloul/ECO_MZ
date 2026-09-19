using ECO.DAL.Interfaces;
using ECO.DAL.Data;
using ECO.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using ECO.DAL.Interfaces.Basket;
using ECO.DAL.Repositories.Basket;
using ECO.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace ECO.DAL
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddDbContext<AppDbContext>(options =>
                     options.UseSqlServer(configuration.GetConnectionString("EcoDataBase"))
                            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            services.AddScoped<ICustomerBasketRepository, CustomerBasketRepository>();
            services.AddSingleton<IConnectionMultiplexer>(r =>
            {
                var config = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis"));
                return ConnectionMultiplexer.Connect(config);
            });
            return services;
        }
    }
}
