using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OnlineStore.Infrastructure.Extensions;

/// <summary>
/// Расширение для подключения JWT-аутентификации.
/// </summary>
public static class JwtServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Получаем настройки JWT из конфигурации
        // Это позволяет легко настраивать параметры аутентификации через appsettings.json
        var jwtSettings = configuration.GetSection("Jwt");
        var key = jwtSettings["Key"]!;

        // Настройка аутентификации JWT
        // Используем схему JWT Bearer для аутентификации
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // Проверяем, что издатель токена соответствует ожидаемому
                    ValidateAudience = true, // Проверяем, что аудитория токена соответствует ожидаемой
                    ValidateLifetime = true, // Проверяем срок действия токена
                    ValidateIssuerSigningKey = true, // Проверяем подпись токена
                    ValidIssuer = jwtSettings["Issuer"], // Ожидаемый издатель
                    ValidAudience = jwtSettings["Audience"], // Ожидаемая аудитория
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) // Ключ для проверки подписи
                };
            });
        // Возвращаем коллекцию сервисов для дальнейшей конфигурации
        // Это позволяет цепочечным вызовам добавлять другие сервисы
        return services;
    }
}
