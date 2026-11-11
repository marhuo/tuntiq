using TuntiQ.Application.Dimensions.Messages;
using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;

namespace TuntiQ.Application.Dimensions.Services;

public class DimensionValueService(
    IDimensionValueRepository dimensionValueRepository,
    IDimensionRepository dimensionRepository,
    DimensionValueValidator dimensionValueValidator)
{
    public async Task<Guid> CreateAsync(DimensionValueCreateRequest request)
    {
        var dimension = await dimensionRepository.GetByIdAsync(request.DimensionId);
        if (dimension is null)
            throw ValidationException.NotFound(nameof(request.DimensionId), "Dimension", "Dimensio");

        await dimensionValueValidator.ValidateCreateAsync(request);

        var value = DimensionValue.Create(dimension, request.NameEn, request.NameFi, request.ReferenceId);

        await dimensionValueRepository.CreateAsync(value);

        return value.Id;
    }

    public async Task UpdateAsync(DimensionValueUpdateRequest request)
    {
        var value = await dimensionValueRepository.GetByIdAsync(request.Id);
        if (value is null)
            throw ValidationException.NotFound();

        await dimensionValueValidator.ValidateUpdateAsync(request, value.DimensionId);

        value.SetNames(request.NameEn, request.NameFi);
        value.SetEnabled(request.Enabled);

        await dimensionValueRepository.UpdateAsync(value);
    }

    public async Task DeleteAsync(Guid id)
    {
        var value = await dimensionValueRepository.GetByIdAsync(id);
        if (value is null)
            throw ValidationException.NotFound();

        value.Delete();

        await dimensionValueRepository.UpdateAsync(value);
    }

    public async Task<PagedListDto<DimensionValueDto>> GetPagedListAsync(DimensionValuePagedListRequest request)
    {
        var pagedList = await dimensionValueRepository.GetPagedListAsync(new DimensionValuePagedListQuery
        {
            DimensionId = request.DimensionId,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SearchTerm = request.SearchTerm,
            Enabled = request.Enabled,
            Language = request.Language,
            OrderDirection = request.OrderDirection.ToDomain()
        });

        return pagedList.ToDto(DimensionValueDto.ToDto);
    }

    public async Task<DimensionValueDto?> GetByIdAsync(Guid id)
    {
        var value = await dimensionValueRepository.GetByIdAsync(id);
        return value is null ? null : DimensionValueDto.ToDto(value);
    }

    public async Task<List<DimensionValueDto>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var values = await dimensionValueRepository.GetByIdsAsync(ids);
        return values.Select(DimensionValueDto.ToDto).ToList();
    }
}
