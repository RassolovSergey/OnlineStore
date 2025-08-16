using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.Application.Exceptions;

namespace OnlineStore.Api.Middlewares;

/// <summary>
/// Глобальный обработчик ошибок.
/// Перехватывает исключения и возвращает RFC7807 ProblemDetails в формате application/problem+json.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    // Конструктор принимает делегат для следующего middleware в конвейере
    // (например, MVC, Razor Pages и т.д.), который будет вызван после обработки
    private readonly RequestDelegate _next;

    // Настройки сериализации JSON для ProblemDetails
    // Используем JsonSerializerDefaults.Web для совместимости с RFC7807
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        // Можно добавить настройку PropertyNamingPolicy = JsonNamingPolicy.CamelCase (уже включено в Defaults.Web)
        WriteIndented = false
    };

    // Конструктор принимает делегат для следующего middleware в конвейере
    // и сохраняет его для вызова после обработки исключений
    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    // Метод Invoke вызывается для каждого HTTP-запроса
    // Здесь мы оборачиваем вызов следующего middleware в try-catch
    public async Task Invoke(HttpContext context)
    {
        try
        {
            // Вызываем следующий middleware в конвейере
            await _next(context);
        }
        catch (AppException appEx)
        {
            // Преобразуем HttpStatusCode -> int для ProblemDetails
            var status = (int)appEx.StatusCode;
            // Логируем appEx (ILogger), если нужно
            // Возвращаем ProblemDetails с соответствующим статусом и сообщением
            await WriteProblem(context, statusCode: status, title: GetTitle(status), detail: appEx.Message);
        }
        catch (Exception ex) // «непредсказуемые» — это 500
        {
            // Логируйте здесь ex (ILogger), чтобы не терять стек.
            await WriteProblem(context, statusCode: StatusCodes.Status500InternalServerError, title: "Внутренняя ошибка сервера", detail: ex.Message);
        }
    }

    // Метод для получения заголовка по статусу
    // Используем switch expression для более лаконичного кода
    private static string GetTitle(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Некорректный запрос",
        StatusCodes.Status401Unauthorized => "Не авторизован",
        StatusCodes.Status403Forbidden => "Доступ запрещён",
        StatusCodes.Status404NotFound => "Не найдено",
        StatusCodes.Status409Conflict => "Конфликт",
        _ => "Ошибка"
    };

    // Метод для записи ProblemDetails в ответ
    // Используем JsonSerializer для сериализации в JSON
    private static async Task WriteProblem(HttpContext ctx, int statusCode, string title, string detail)
    {
        // Создаём ProblemDetails с нужными полями
        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",    // Ссылка на описание статуса
            Title = title,  // Заголовок ошибки
            Status = statusCode,    // HTTP-статус
            Detail = detail,    // Подробное описание ошибки
            Instance = ctx.Request.Path // Путь запроса, где произошла ошибка
        };

        // Доп. сведения — удобно для трассировки
        problem.Extensions["traceId"] = ctx.TraceIdentifier;

        // Устанавливаем заголовки ответа
        ctx.Response.ContentType = MediaTypeNames.Application.ProblemJson;  // application/problem+json
        ctx.Response.StatusCode = statusCode;   // Устанавливаем HTTP-статус

        // Сериализуем ProblemDetails в JSON и записываем в ответ
        // Используем JsonSerializer с настройками для совместимости с RFC7807
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
    }
}
