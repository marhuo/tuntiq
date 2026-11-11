using TuntiQ.Application.Dimensions.Messages;
using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;

namespace TuntiQ.Application.Dimensions.Services;

public class DimensionValueValidator(IDimensionValueRepository dimensionValueRepository)
{
    public async Task ValidateCreateAsync(DimensionValueCreateRequest request)
    {
        await ValidateNamesAsync(request, request.DimensionId);
    }

    /// <param name="dimensionId">Dimension of the stored value; the update request does not carry it.</param>
    public async Task ValidateUpdateAsync(DimensionValueUpdateRequest request, Guid dimensionId)
    {
        await ValidateNamesAsync(request, dimensionId, request.Id);
    }

    /// <summary>
    /// A name may be used only once within a dimension, in either language.
    /// </summary>
    private async Task ValidateNamesAsync(ILocalizedNameDto dto, Guid dimensionId, Guid? id = null)
    {
        var errors = new ErrorCollector();
        errors.Required(nameof(dto.NameFi), dto.NameFi, "Name (Finnish)", "Nimi (suomi)");
        errors.Required(nameof(dto.NameEn), dto.NameEn, "Name (English)", "Nimi (englanti)");
        errors.MaxLength(nameof(dto.NameFi), dto.NameFi, "Name (Finnish)", "Nimi (suomi)", 0, DimensionValue.MaxNameLength);
        errors.MaxLength(nameof(dto.NameEn), dto.NameEn, "Name (English)", "Nimi (englanti)", 0, DimensionValue.MaxNameLength);
        errors.ThrowIfAny();

        if (await dimensionValueRepository.GetByNameAsync(dimensionId, dto.NameFi, id) is not null)
            errors.Add(ValidationErrors.Taken(nameof(dto.NameFi), "Name (Finnish)", "Nimi (suomi)"));

        if (await dimensionValueRepository.GetByNameAsync(dimensionId, dto.NameEn, id) is not null)
            errors.Add(ValidationErrors.Taken(nameof(dto.NameEn), "Name (English)", "Nimi (englanti)"));

        errors.ThrowIfAny();
    }
}
