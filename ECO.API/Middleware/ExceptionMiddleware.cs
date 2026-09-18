using ECO.Api.Helper;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text.Json;

namespace ECO.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        private readonly IMemoryCache _memoryCache;
        private readonly TimeSpan _RateLimitWindow= TimeSpan.FromSeconds(30);
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env,IMemoryCache memoryCache)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _memoryCache = memoryCache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                ApplaySecurity(context);
                if (!IsRequestAllowed(context))
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    var response = new ExcepationsApi((int)HttpStatusCode.TooManyRequests, "Too many requests. Please try again later.");
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    var json = JsonSerializer.Serialize(response, jsonOptions);
                    await context.Response.WriteAsync(json);
                    return;
                }
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception: {Message}", ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()? new ExcepationsApi(
                        (int)HttpStatusCode.InternalServerError, ex.Message,ex.StackTrace): 
                        new ExcepationsApi((int)HttpStatusCode.InternalServerError, "An unexpected internal server error occurred.");

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(response, jsonOptions);
                await context.Response.WriteAsync(json);
            }
        }
        private bool IsRequestAllowed(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var cachKey=$"RateLimit_{ip}";
            var dateNow = DateTime.UtcNow;
            var (timestamp, count) = _memoryCache.GetOrCreate(cachKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _RateLimitWindow;
                return (dateNow, 0);
            });
            if (dateNow - timestamp < _RateLimitWindow)
            {
                if (count >= 60)
                {
                    return false;
                }
                _memoryCache.Set(cachKey, (timestamp, count += 1), _RateLimitWindow); 
            }
            else
            {
                _memoryCache.Set(cachKey, (timestamp, count), _RateLimitWindow);
            }
            


            return true;
        }
        private void ApplaySecurity(HttpContext context)
        {
          context.Response.Headers["X-Content-Type-Options"] = "nosniff";
          context.Response.Headers["X-XSS-Production"] = "1; mode=block";
          context.Response.Headers["X-Frame-Options"] = "DENY";

        }
    }
}
