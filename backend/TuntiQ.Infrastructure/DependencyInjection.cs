using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;
using TuntiQ.Domain.DimensionValues;
using TuntiQ.Infrastructure.Events;
using TuntiQ.Infrastructure.Persistence;

namespace TuntiQ.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TuntiQDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<IDimensionRepository, DimensionRepository>();
        services.AddScoped<IDimensionValueRepository, DimensionValueRepository>();

        return services;
    }
}
