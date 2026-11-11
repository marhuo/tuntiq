namespace TuntiQ.Domain.Dimensions;

public interface IDimensionRepository
{
    Task<List<Dimension>> GetAllAsync();
    Task<Dimension?> GetByIdAsync(Guid id);
    Task<List<Dimension>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task CreateAsync(Dimension dimension);
    Task UpdateAsync(Dimension dimension);
}
