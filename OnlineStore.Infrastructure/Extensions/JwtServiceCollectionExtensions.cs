using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OnlineStore.Infrastructure.Options;

namespace OnlineStore.Infrastructure.Extensions;

public static class JwtServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // 1) Биндим и валидируем опции
        var section = configuration.GetSection("Jwt");
        services.AddOptions<JwtOptions>()
            .Bind(section)
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is required")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "Jwt:Key is required")
            .Validate(o => Encoding.UTF8.GetBytes(o.Key).Length >= 32, "Jwt:Key must be at least 32 bytes (256 bits)")
            .Validate(o => o.AccessTokenLifetimeMinutes > 0, "Jwt:AccessTokenLifetimeMinutes must be positive")
            .ValidateOnStart(); // Валидируем на старте. Для того, 
            // чтобы сразу увидеть ошибки конфигурации и лишний раз не запускать приложение с некорректными настройками.

        // 2) Достаём актуальные значения (при старте)
        var opts = section.Get<JwtOptions>()!;
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key));

        // 3) Подключаем аутентификацию JWT
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero, // без лишних 5 минут
                    ValidIssuer = opts.Issuer,
                    ValidAudience = opts.Audience,
                    IssuerSigningKey = signingKey
                };
            });

        return services;
    }
}
