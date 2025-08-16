using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace OnlineStore.Api.Extensions;

/// <summary>
/// Расширения для настройки и включения rate limiting в веб-слое.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Регистрирует политики лимитов для /auth/login и /auth/register (пер-IP), 
    /// а также единый обработчик 429 в формате ProblemDetails.
    /// </summary>
    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Единый ответ при превышении лимита: 429 application/problem+json
            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;

                var retryAfter = context.Lease?.TryGetMetadata(MetadataName.RetryAfter, out var ra) == true
                    ? ra.TotalSeconds.ToString("F0")
                    : "60";
                context.HttpContext.Response.Headers.Append("Retry-After", retryAfter);

                var pd = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/429",
                    Title = "Слишком много запросов",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "Повторите попытку позже.",
                    Instance = context.HttpContext.Request.Path
                };
                pd.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(pd), ct);
            };

            // Политика для /auth/login — 10 запросов/мин на IP
            options.AddPolicy("ip-login", httpContext =>
            {
                var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            // Политика для /auth/register — 3 запроса/мин на IP
            options.AddPolicy("ip-register", httpContext =>
            {
                var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }

    /// <summary>
    /// Подключает middleware лимитирования запросов.
    /// </summary>
    public static IApplicationBuilder UseAuthRateLimiting(this IApplicationBuilder app)
        => app.UseRateLimiter();
}
