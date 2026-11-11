using TuntiQ.Application.Dimensions.Messages;
using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;

namespace TuntiQ.Application.Dimensions.Services;

public class DimensionService(IDimensionRepository dimensionRepository, DimensionValidator dimensionValidator)
{
    public async Task<Guid> CreateAsync(DimensionCreateRequest request)
    {
        await dimensionValidator.ValidateCreateAsync(request);

        Guid tenantId = Guid.CreateVersion7();
        var dimension = Dimension.Create(tenantId, request.Kind.ToDomain(), request.NameEn, request.NameFi, request.AllowMultiple, request.IsMandatory);
        
        await dimensionRepository.CreateAsync(dimension);

        return dimension.Id;
    }

    public async Task Update(DimensionUpdateRequest request)
    {
        var dimension = await dimensionRepository.GetByIdAsync(request.Id);
        if (dimension is null)
            throw ValidationException.NotFound();

        await dimensionValidator.ValidateUpdateAsync(request);

        dimension.SetNames(request.NameEn, request.NameFi);
        dimension.SetEnabled(request.Enabled);
        dimension.SetAllowMultiple(request.AllowMultiple);
        dimension.SetIsMandatory(request.IsMandatory);

        await dimensionRepository.UpdateAsync(dimension);
    }

    public async Task<PagedListDto<DimensionDto>> GetPagedListAsync(DimensionPagedListRequest request)
    {
        IEnumerable<Dimension> dimensions = await dimensionRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            dimensions = dimensions.Where(d =>
                d.NameFi.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.NameEn.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        dimensions = request.OrderBy == DimensionDtoOrderBy.NameEn
            ? dimensions.OrderBy(d => d.NameEn)
            : dimensions.OrderBy(d => d.NameFi);

        var pagedList = dimensions.ToList().ToPagedList(request.PageNumber, request.PageSize);
        return pagedList.ToDto(DimensionDto.ToDto);
    }

    public async Task<DimensionDto?> GetById(Guid id)
    {
        var dimension = await dimensionRepository.GetByIdAsync(id);
        if (dimension is null) return null;

        return DimensionDto.ToDto(dimension);
    }

    public async Task<List<DimensionDto>?> GetByIds(IEnumerable<Guid> ids)
    {
        var dimensions = await dimensionRepository.GetByIdsAsync(ids);
        return dimensions.Select(DimensionDto.ToDto).ToList();
    }
}
