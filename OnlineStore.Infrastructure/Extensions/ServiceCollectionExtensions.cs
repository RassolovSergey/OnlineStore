using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using OnlineStore.Domain.Entities;
using OnlineStore.Application.Interfaces.Repositories;
using OnlineStore.Application.Interfaces.Services;
using OnlineStore.Infrastructure.Repositories;
using OnlineStore.Infrastructure.Services;
using OnlineStore.Infrastructure.Security;

namespace OnlineStore.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Репозитории
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            // Сервисы
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IAuthService, AuthService>();

            // Вспомогательные зависимости
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IRefreshTokenFactory, RefreshTokenFactory>();
            return services;
        }
    }
}