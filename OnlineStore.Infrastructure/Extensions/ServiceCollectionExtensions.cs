using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using OnlineStore.Domain.Entities;
using OnlineStore.Application.Interfaces.Repositories;
using OnlineStore.Application.Interfaces.Services;
using OnlineStore.Infrastructure.Repositories;
using OnlineStore.Infrastructure.Services;

namespace OnlineStore.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Репозитории, которые УЖЕ существуют
        services.AddScoped<IUserRepository, UserRepository>();

        // Инфраструктурные сервисы (если AuthService перенесён в Infrastructure/Services)
        services.AddScoped<IAuthService, AuthService>();

        // Вспомогательные зависимости
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}
