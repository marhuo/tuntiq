using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;
using TuntiQ.Infrastructure.Persistence;
using TuntiQ.Infrastructure.Persistence.Ef;
using Microsoft.EntityFrameworkCore;

public class DimensionRepository(TuntiQDbContext db, IDomainEventDispatcher eventDispatcher) : IDimensionRepository
{
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

    public async Task<List<Dimension>> GetAllAsync()
    {
        var entities = await db.Dimensions.AsNoTracking()
            .Include(d => d.Names)
            .ToListAsync();

        return entities.Select(e => e.ToDomain()).ToList();
    }

    private async Task SaveAsync(Dimension dimension)
    {
        var domainEvents = dimension.GetDomainEvents();

        foreach (var domainEvent in domainEvents)
            db.DimensionEvents.Add(DimensionEventEntity.FromDomain(dimension.Id, dimension.TenantId, domainEvent));

        await db.SaveChangesAsync();

        dimension.ClearDomainEvents();
        await eventDispatcher.Dispatch(domainEvents);
    }
}