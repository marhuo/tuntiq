using Microsoft.EntityFrameworkCore;
using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;
using TuntiQ.Domain.DimensionValues;
using TuntiQ.Infrastructure.Persistence.Ef;

namespace TuntiQ.Infrastructure.Persistence;

public class DimensionValueRepository(TuntiQDbContext db, IDomainEventDispatcher eventDispatcher) : IDimensionValueRepository
{
    public async Task CreateAsync(DimensionValue dimensionValue)
    {
        db.DimensionValues.Add(DimensionValueEntity.FromDomain(dimensionValue));
        await SaveAsync(dimensionValue);
    }

    public async Task UpdateAsync(DimensionValue dimensionValue)
    {
        if (dimensionValue.GetDomainEvents().Count == 0)
            return;

        var entity = await db.DimensionValues
            .Include(e => e.Names)
            .SingleOrDefaultAsync(e => e.Id == dimensionValue.Id)
            ?? throw ValidationException.NotFound();

        entity.UpdateFrom(dimensionValue);
        await SaveAsync(dimensionValue);
    }

    public async Task<DimensionValue?> GetByIdAsync(Guid id)
    {
        var entity = await db.DimensionValues.AsNoTracking()
            .Include(e => e.Names)
            .SingleOrDefaultAsync(e => e.Id == id);

        return entity?.ToDomain();
    }

    public async Task<List<DimensionValue>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return [];

        var entities = await db.DimensionValues.AsNoTracking()
            .Include(e => e.Names)
            .Where(e => idList.Contains(e.Id))
            .ToListAsync();

        return entities.Select(e => e.ToDomain()).ToList();
    }

    public async Task<PagedList<DimensionValue>> GetPagedListAsync(DimensionValuePagedListQuery query)
    {
        var language = query.Language == "en" ? "en" : "fi";

        IQueryable<DimensionValueEntity> source = db.DimensionValues.AsNoTracking()
            .Include(e => e.Names);

        if (query.Enabled is not null)
            source = source.Where(e => e.Enabled == query.Enabled);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm) && query.SearchTerm.Trim().Length >= 3)
        {
            var pattern = $"%{query.SearchTerm.Trim()}%";
            source = source.Where(e => e.Names.Any(n => EF.Functions.ILike(n.Name, pattern)));
        }

        source = query.OrderDirection == OrderDirections.Desc
            ? source.OrderByDescending(e => e.Names.Where(n => n.Language == language).Select(n => n.Name).FirstOrDefault())
            : source.OrderBy(e => e.Names.Where(n => n.Language == language).Select(n => n.Name).FirstOrDefault());

        var pageNumber = Math.Max(query.PageNumber, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await source.CountAsync();
        var entities = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedList<DimensionValue>(
            entities.Select(e => e.ToDomain()).ToList(),
            pageNumber,
            pageSize,
            totalCount);
    }

    private async Task SaveAsync(DimensionValue dimensionValue)
    {
        var domainEvents = dimensionValue.GetDomainEvents();

        foreach (var domainEvent in domainEvents)
            db.DimensionValueEvents.Add(DimensionValueEventEntity.FromDomain(dimensionValue.Id, dimensionValue.TenantId, domainEvent));

        await db.SaveChangesAsync();

        dimensionValue.ClearDomainEvents();
        await eventDispatcher.Dispatch(domainEvents);
    }
}