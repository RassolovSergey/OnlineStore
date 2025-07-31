using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OnlineStore.Application.Interfaces;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Repositories;
using OnlineStore.Infrastructure.Services;

namespace OnlineStore.Infrastructure.Extensions;

/// <summary>
/// Расширения для регистрации сервисов и репозиториев в DI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрация всех инфраструктурных зависимостей.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Регистрация репозитория продуктов
        // Используется для внедрения зависимостей в контроллеры и сервисы
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Регистрация сервиса продуктов
        // Используется для бизнес-логики и обработки DTO
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();

        // Вспомогательные зависимости
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}
