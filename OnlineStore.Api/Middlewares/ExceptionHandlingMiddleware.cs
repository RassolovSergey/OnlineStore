using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Exceptions;

namespace OnlineStore.Api.Middlewares
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (UnauthorizedAppException ex)
            {
                await WriteProblem(ctx, StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message);
            }
            catch (ForbiddenAppException ex)
            {
                await WriteProblem(ctx, StatusCodes.Status403Forbidden, "Forbidden", ex.Message);
            }
            catch (NotFoundException ex)
            {
                await WriteProblem(ctx, StatusCodes.Status404NotFound, "Not Found", ex.Message);
            }
            catch (ConflictAppException ex)
            {
                await WriteProblem(ctx, StatusCodes.Status409Conflict, "Conflict", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled");
                await WriteProblem(ctx, StatusCodes.Status500InternalServerError, "Server error",
                    "Произошла непредвиденная ошибка.");
            }
        }

        // Пока не используется, но может пригодиться, оставил.
        private static string GetTitle(int status) => status switch
        {
            StatusCodes.Status400BadRequest => "Некорректный запрос",
            StatusCodes.Status401Unauthorized => "Не авторизован",
            StatusCodes.Status403Forbidden => "Доступ запрещён",
            StatusCodes.Status404NotFound => "Не найдено",
            StatusCodes.Status409Conflict => "Конфликт",
            _ => "Ошибка"
        };

        // Helper
        private static async Task WriteProblem(HttpContext ctx, int statusCode, string title, string detail)
        {
            // Заголовки до записи тела
            ctx.Response.ContentType = MediaTypeNames.Application.ProblemJson;
            ctx.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{statusCode}",
                Title = title,
                Status = statusCode,
                Detail = detail,
                Instance = ctx.Request.Path
            };
            problem.Extensions["traceId"] = ctx.TraceIdentifier;

            var payload = JsonSerializer.Serialize(problem, JsonOpts);
            await ctx.Response.WriteAsync(payload);
        }
    }
}
