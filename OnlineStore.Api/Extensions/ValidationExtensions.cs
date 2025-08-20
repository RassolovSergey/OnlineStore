using FluentValidation;                                   // регистрируем валидаторы
using FluentValidation.AspNetCore;                      // интеграция с ASP.NET Core
using Microsoft.AspNetCore.Mvc;                         // ApiBehaviorOptions
using Microsoft.Extensions.DependencyInjection;
using OnlineStore.Application.Validation.Auth;
using System.Linq;

namespace OnlineStore.Api.Extensions
{
    /// <summary>
    /// Расширения для подключения FluentValidation и унификации ответа 400 (ProblemDetails).
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Подключает FluentValidation: авто-валидацию и регистрацию валидаторов из сборки Application.
        /// Также настраивает единый ответ 400 с детализированным списком ошибок.
        /// </summary>
        public static IServiceCollection AddAppValidation(this IServiceCollection services)
        {
            // 1) Подключаем авто-валидацию: FluentValidation выполняется автоматически при биндинге моделей
            services.AddFluentValidationAutoValidation();

            // 2) Ищем и регистрируем все валидаторы из сборки, где находится выбранный валидатор.
            //    Выберите любой валидатор из вашего слоя Application, чтобы заставить сканировать ту сборку.
            services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

            // 3) Настраиваем единый ответ при невалидной модели (400) в стиле ProblemDetails.
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    // Собираем ошибки в удобный вид: field -> [messages]
                    var errors = context.ModelState
                        .Where(kvp => kvp.Value?.Errors?.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,                                       // имя поля/пути
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage)  // сообщения
                        );

                    // Формируем стандартный ProblemDetails на 400
                    var problem = new ValidationProblemDetails(context.ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation Failed",
                        Detail = "One or more validation errors occurred.",
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1" // для вида
                    };

                    // По желанию: можно положить «плоский» объект с ошибками в Extensions.
                    // Тогда фронту проще показывать ошибки по полям.
                    problem.Extensions["errorsByField"] = errors;

                    return new BadRequestObjectResult(problem)
                    {
                        ContentTypes = { "application/problem+json", "application/problem+xml" }
                    };
                };
            });

            return services;
        }
    }
}
