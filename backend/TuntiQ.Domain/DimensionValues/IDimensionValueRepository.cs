using TuntiQ.Domain.Dimensions;

namespace TuntiQ.Domain.DimensionValues;

public interface IDimensionValueRepository
{
    Task<DimensionValue?> GetByIdAsync(Guid id);
    Task<List<DimensionValue>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task CreateAsync(DimensionValue dimensionValue);
    Task UpdateAsync(DimensionValue dimensionValue);
    Task<PagedList<DimensionValue>> GetPagedListAsync(DimensionValuePagedListQuery query);
}
