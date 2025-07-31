using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace OnlineStore.Application.Extensions;

public static class MappingServiceCollectionExtensions
{
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingServiceCollectionExtensions).Assembly);
        return services;
    }
}
