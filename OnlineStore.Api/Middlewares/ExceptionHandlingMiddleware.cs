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

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException appEx)
            {
                // Логируем без стека как Warning (это бизнес-ошибка, ожидаемая)
                _logger.LogWarning(appEx, "AppException {Status} at {Path}: {Message}",
                    (int)appEx.StatusCode, context.Request.Path, appEx.Message);

                var status = (int)appEx.StatusCode;
                await WriteProblem(context, status, GetTitle(status), appEx.Message);
            }
            catch (Exception ex)
            {
                // Логируем со стеком как Error (неожиданная ошибка)
                _logger.LogError(ex, "Unhandled exception at {Path}", context.Request.Path);

                // В проде лучше не отдавать ex.Message наружу. Можно отдать общее сообщение.
                const string generic = "Внутренняя ошибка сервера";
                await WriteProblem(context, StatusCodes.Status500InternalServerError, generic, generic);
            }
        }

        private static string GetTitle(int status) => status switch
        {
            StatusCodes.Status400BadRequest => "Некорректный запрос",
            StatusCodes.Status401Unauthorized => "Не авторизован",
            StatusCodes.Status403Forbidden => "Доступ запрещён",
            StatusCodes.Status404NotFound => "Не найдено",
            StatusCodes.Status409Conflict => "Конфликт",
            _ => "Ошибка"
        };

        private static async Task WriteProblem(HttpContext ctx, int statusCode, string title, string detail)
        {
            var problem = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{statusCode}",
                Title = title,
                Status = statusCode,
                Detail = detail,
                Instance = ctx.Request.Path
            };

            problem.Extensions["traceId"] = ctx.TraceIdentifier;

            ctx.Response.ContentType = MediaTypeNames.Application.ProblemJson;
            ctx.Response.StatusCode = statusCode;

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
        }
    }
}