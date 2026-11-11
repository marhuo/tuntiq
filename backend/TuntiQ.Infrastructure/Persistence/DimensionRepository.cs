using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;
using TuntiQ.Infrastructure.Caching;
using TuntiQ.Infrastructure.Persistence.Ef;

namespace TuntiQ.Infrastructure.Persistence;

public class DimensionRepository(TuntiQDbContext db, IDomainEventDispatcher eventDispatcher, IMemoryCache cache) : IDimensionRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task CreateAsync(Dimension dimension)
    {
        db.Dimensions.Add(DimensionEntity.FromDomain(dimension));
        await SaveAsync(dimension);
    }

    public async Task UpdateAsync(Dimension dimension)
    {
        if (!dimension.GetDomainEvents().Any())
            return;

        var entity = await db.Dimensions
            .Include(d => d.Names)
            .SingleOrDefaultAsync(d => d.Id == dimension.Id)
            ?? throw ValidationException.NotFound();

        entity.UpdateFrom(dimension);
        await SaveAsync(dimension);
    }

    public async Task<Dimension?> GetByIdAsync(Guid id)
    {
        var entity = await db.Dimensions.AsNoTracking()
            .Include(d => d.Names)
            .SingleOrDefaultAsync(d => d.Id == id);

        return entity?.ToDomain();
    }

    public async Task<List<Dimension>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return [];

        var entities = await db.Dimensions.AsNoTracking()
            .Include(d => d.Names)
            .Where(d => idList.Contains(d.Id))
            .ToListAsync();

        return entities.Select(e => e.ToDomain()).ToList();
    }

    /// <summary>
    /// Returns all dimensions of the current tenant. The result is cached per tenant
    /// and invalidated in <see cref="SaveAsync"/>.
    /// </summary>
    public async Task<List<Dimension>> GetAllAsync()
    {
        var tenantId = CurrentUser.TenantId;

        // System user sees all tenants; do not cache a cross-tenant result.
        if (tenantId is null || CurrentUser.IsSystemUser)
            return await LoadAllAsync();

        return await cache.GetOrCreateSerializedAsync(CacheKey(tenantId.Value), CacheDuration, LoadAllAsync);
    }

    private async Task<List<Dimension>> LoadAllAsync()
    {
        var entities = await db.Dimensions.AsNoTracking()
            .Include(d => d.Names)
            .ToListAsync();

        return entities.Select(e => e.ToDomain()).ToList();
    }

    private static string CacheKey(Guid tenantId) => $"dimensions:{tenantId}";

    private async Task SaveAsync(Dimension dimension)
    {
        var domainEvents = dimension.GetDomainEvents();

        foreach (var domainEvent in domainEvents)
            db.DimensionEvents.Add(DimensionEventEntity.FromDomain(dimension.Id, dimension.TenantId, domainEvent));

        await db.SaveChangesAsync();
        cache.Remove(CacheKey(dimension.TenantId));

        dimension.ClearDomainEvents();
        await eventDispatcher.Dispatch(domainEvents);
    }
}