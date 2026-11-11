namespace TuntiQ.Domain.Dimensions;

public interface IDimensionValueRepository
{
    Task<DimensionValue?> GetByIdAsync(Guid id);
    Task<List<DimensionValue>> GetByIdsAsync(IEnumerable<Guid> ids);

    /// <summary>
    /// Finds a value of the dimension whose name in any language equals <paramref name="name"/>,
    /// ignoring case. <paramref name="excludeId"/> skips the value being updated.
    /// </summary>
    Task<DimensionValue?> GetByNameAsync(Guid dimensionId, string name, Guid? excludeId = null);

    Task CreateAsync(DimensionValue dimensionValue);
    Task UpdateAsync(DimensionValue dimensionValue);
    Task<PagedList<DimensionValue>> GetPagedListAsync(DimensionValuePagedListQuery query);
}
