using Microsoft.Extensions.DependencyInjection;
using TuntiQ.Application.Dimensions.Services;

namespace TuntiQ.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<DimensionService>();
        services.AddScoped<DimensionValidator>();
        return services;
    }
}