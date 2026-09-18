using AutoMapper;
using ECO.Api.Middleware;
using ECO.BLL;
using ECO.DAL;
using ECO.DAL.Data;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostFilteringOptions>(options =>
{
    options.AllowedHosts = new[]
    {
        "localhost",
        "127.0.0.1",
        "localhost:4200",
        "127.0.0.1:4200",
        "localhost:7085",
        "127.0.0.1:7085"
    };
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CrosPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://127.0.0.1:4200",
                "https://127.0.0.1:4200",
                "https://localhost:7085",
                "http://localhost:7085")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add DAL services 
builder.Services.AddInfrastractionConfiguration(builder.Configuration);

// Add BLL services 
builder.Services.AddApplicationServices();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Apply Migrations and Seed Data on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await AppDbContextSeed.SeedAsync(context, loggerFactory);
        logger.LogInformation("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration/seeding.");
    }
}

app.UseCors("CrosPolicy");

// Error Handling Middleware
app.UseStatusCodePagesWithReExecute("/error/{0}");
// Global Exception Handling Middleware
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
