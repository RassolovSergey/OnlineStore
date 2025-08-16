using Microsoft.Extensions.DependencyInjection;

namespace OnlineStore.Application.Extensions;

/// <summary>
/// Регистрация только AutoMapper-профилей из сборки Application.
/// Никаких инфраструктурных сервисов тут быть не должно.
/// </summary>
public static class MappingServiceCollectionExtensions
{
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        // Подтянуть все Profile из текущей сборки (Application)
        services.AddAutoMapper(typeof(MappingServiceCollectionExtensions).Assembly);
        return services;
    }
}
