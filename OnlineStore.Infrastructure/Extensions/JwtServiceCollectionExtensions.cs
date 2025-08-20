using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OnlineStore.Infrastructure.Options;

namespace OnlineStore.Infrastructure.Extensions
{
    public static class JwtServiceCollectionExtensions
    {
        /// <summary>
        /// ЕДИНСТВЕННАЯ точка регистрации JWT-аутентификации ("Bearer").
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // 1) Bind + validate JwtOptions из конфигурации
            var section = configuration.GetSection("Jwt");

            services.AddOptions<JwtOptions>()
                .Bind(section)
                .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && Encoding.UTF8.GetByteCount(o.Key) >= 32,
                    "Jwt:Key must be at least 32 bytes.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Jwt:Issuer is required.")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Jwt:Audience is required.")
                .ValidateOnStart();

            // 2) Настраиваем аутентификацию и JwtBearer (БЕЗ перегрузки с IServiceProvider)
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    // читаем JwtOptions напрямую из конфигурации (их уже провалидировали выше)
                    var jwt = section.Get<JwtOptions>()!;

                    // Определяем Dev-окружение из конфигурации (без доступа к IHostEnvironment)
                    var isDev = string.Equals(
                        configuration["ASPNETCORE_ENVIRONMENT"],
                        "Development",
                        StringComparison.OrdinalIgnoreCase);

                    options.RequireHttpsMetadata = !isDev; // в Dev разрешаем без HTTPS метаданных

                    options.SaveToken = false;        // не сохраняем токен во внутренние куки
                    options.MapInboundClaims = false; // не ремапим типы клеймов

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,

                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),

                        // Явно указываем типы клеймов
                        NameClaimType = ClaimTypes.NameIdentifier,
                        RoleClaimType = ClaimTypes.Role
                    };
                });

            return services;
        }
    }
}
