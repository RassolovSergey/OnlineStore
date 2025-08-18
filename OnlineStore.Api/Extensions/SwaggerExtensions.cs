using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace OnlineStore.Api.Extensions
{
    /// <summary>
    /// Расширения для настройки Swagger с поддержкой JWT (Authorize).
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Регистрирует SwaggerGen и добавляет схему безопасности "Bearer".
        /// </summary>
        public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
        {
            // Добавляем SwaggerGen для генерации документации API
            services.AddSwaggerGen(c =>
            {
                // Базовая инфа о документе (по желанию можно вынести в конфиг)
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "OnlineStore API",
                    Version = "v1"
                });

                // Описываем схему безопасности: HTTP Bearer, формат JWT
                var jwtScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Введите **только** JWT-токен без префикса `Bearer ` — Swagger подставит его сам."
                };

                // Добавляем схему в Swagger
                c.AddSecurityDefinition("Bearer", jwtScheme);

                // Глобальное требование наличия схемы (заметку об авторизации увидят все операции)
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme { Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                    Array.Empty<string>()
                }
                });
            });

            return services;
        }
    }
}