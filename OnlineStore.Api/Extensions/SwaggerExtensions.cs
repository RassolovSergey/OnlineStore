using System.IO;
using System.Reflection;
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
        /// Регистрирует SwaggerGen, добавляет схему безопасности "Bearer"
        /// и подключает XML-комментарии (summary/remarks) из сборки API.
        /// </summary>
        public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                // Базовая информация о документе
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "OnlineStore API",
                    Version = "v1"
                });

                // HTTP Bearer схема для JWT
                var jwtScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Введите **только** JWT-токен без префикса `Bearer ` — Swagger подставит его сам."
                };

                c.AddSecurityDefinition("Bearer", jwtScheme);

                // Глобальное требование наличия схемы авторизации
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // === Подключение XML-комментариев ===
                // Включи в csproj генерацию XML: <GenerateDocumentationFile>true</GenerateDocumentationFile>
                // Тогда summary/remarks из контроллеров и моделей попадут в Swagger UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    // includeControllerXmlComments:true — чтобы тянуть xml-комменты и для контроллеров
                    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            });

            return services;
        }
    }
}
