using AutoMapper;

using ECO.Api.Middleware;

using ECO.BLL;

using ECO.DAL;

using ECO.DAL.Data;

using ECO.DAL.Entities;

using ECO.BLL.Services.Basket;

using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.AspNetCore.HostFiltering;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using Microsoft.IdentityModel.Tokens;

using System.Text;



var builder = WebApplication.CreateBuilder(args);



builder.Services.Configure<HostFilteringOptions>(options =>

{

    options.AllowedHosts = builder.Configuration
        .GetSection("AllowedHosts")
        .Get<string[]>()
        ?? new[] { "localhost", "127.0.0.1" };

});



builder.Services.AddControllers();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("guest-orders", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.AddPolicy("landing-analytics", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});



builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>

{

    options.AddPolicy("CorsPolicy", policy =>
    {

        policy

            .WithOrigins(builder.Configuration
                .GetSection("Cors:Origins")
                .Get<string[]>()
                ?? new[]
                {
                    "http://localhost:4200",
                    "https://localhost:4200",
                    "http://127.0.0.1:4200",
                    "https://127.0.0.1:4200"
                })

            .AllowAnyMethod()

            .AllowAnyHeader()

            .AllowCredentials();

    });

});



// Add DAL services 

builder.Services.AddInfrastructureConfiguration(builder.Configuration);


// Add BLL services 

builder.Services.AddApplicationServices();

builder.Services.Configure<BasketSettings>(

    builder.Configuration.GetSection("BasketSettings"));


builder.Services.AddMemoryCache();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().
    AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// Email confirmation / password-reset token lifespan, surfaced in the account emails.
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromMinutes(
        builder.Configuration.GetValue("Auth:EmailTokenLifespanMinutes", 60)));

var jwtSecret = builder.Configuration["Token:Secret"];
var jwtIssuer = builder.Configuration["Token:Issuer"];
if (string.IsNullOrWhiteSpace(jwtSecret) || string.IsNullOrWhiteSpace(jwtIssuer))
    throw new InvalidOperationException("Token:Secret and Token:Issuer must be configured.");

builder.Services.AddAuthentication(op =>
{

    op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

    op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    op.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddCookie(o =>

{

    o.Cookie.Name = "token";

    o.Events.OnRedirectToLogin = context =>

    {

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;

    };

}).AddJwtBearer(op =>

{

    op.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

    op.SaveToken = true;

    op.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters

    {

        ValidateIssuerSigningKey = true,

        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidIssuer = jwtIssuer,
        ValidateIssuer = true,
        ValidateAudience=false,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew=TimeSpan.Zero

    };

    op.Events = new JwtBearerEvents

    {

        OnMessageReceived = context =>

        {

            context.Token = context.Request.Cookies["token"];

            return Task.CompletedTask;

        }

    };



});



// Google social login — enabled only when credentials are configured.

var googleClientId = builder.Configuration["Google:ClientId"];

var googleClientSecret = builder.Configuration["Google:ClientSecret"];

if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))

{

    builder.Services.AddAuthentication().AddGoogle(options =>
    {

        options.ClientId = googleClientId;

        options.ClientSecret = googleClientSecret;

        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.CallbackPath = builder.Configuration["Google:CallbackPath"] ?? "/signin-google";
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Events.OnRemoteFailure = context =>
        {
            var frontendUrl = context.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["FrontendUrl"]?.TrimEnd('/');
            context.Response.Redirect(
                string.IsNullOrWhiteSpace(frontendUrl)
                    ? "/account/login?socialError=failed"
                    : $"{frontendUrl}/account/login?socialError=failed");
            context.HandleResponse();
            return Task.CompletedTask;
        };

    });

}

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



        // Seed the Admin role and promote configured bootstrap emails.

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))

        {

            await roleManager.CreateAsync(new IdentityRole("Admin"));

            logger.LogInformation("Admin role created.");

        }

        var adminEmails = builder.Configuration.GetSection("Admin:BootstrapEmails").Get<string[]>() ?? Array.Empty<string>();

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var adminEmail in adminEmails)

        {

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))

            {

                await userManager.AddToRoleAsync(adminUser, "Admin");

                logger.LogInformation("Admin role granted to {Email}.", adminEmail);

            }

        }



        await AppDbContextSeed.SeedAsync(context, loggerFactory);
        logger.LogInformation("Database migrated and seed data ensured successfully.");

    }
    catch (Exception ex)

    {

        logger.LogError(ex, "An error occurred during database migration/seeding.");

    }

}



app.UseCors("CorsPolicy");


// Error Handling Middleware

app.UseStatusCodePagesWithReExecute("/error/{0}");

// Global Exception Handling Middleware

app.UseMiddleware<ExceptionMiddleware>();

if (!app.Environment.IsDevelopment())
{
    // Only redirect to HTTPS in production — in development the Angular proxy uses HTTP on
    // port 5045 and a 307 redirect breaks the proxy's 401/403 handling.
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();
