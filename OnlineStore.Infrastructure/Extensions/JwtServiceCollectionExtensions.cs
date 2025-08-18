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
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("Jwt");
            services.AddOptions<JwtOptions>()
                .Bind(section)
                .Validate(opts => !string.IsNullOrWhiteSpace(opts.Key) && Encoding.UTF8.GetByteCount(opts.Key) >= 32, "Jwt:Key must be at least 32 bytes.")
                .Validate(opts => !string.IsNullOrWhiteSpace(opts.Issuer), "Jwt:Issuer is required.")
                .Validate(opts => !string.IsNullOrWhiteSpace(opts.Audience), "Jwt:Audience is required.")
                .ValidateOnStart();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var opts = section.Get<JwtOptions>()!;
                    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opts.Key));

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = opts.Issuer,
                        ValidAudience = opts.Audience,
                        IssuerSigningKey = signingKey
                    };
                });

            return services;
        }
    }
}
