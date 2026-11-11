using TuntiQ.Application.Dimensions.Messages;
using TuntiQ.Domain.Common;
using TuntiQ.Domain.Dimensions;

namespace TuntiQ.Application.Dimensions.Services;

public class DimensionValidator(IDimensionRepository dimensionRepository)
{
    public async Task ValidateCreateAsync(DimensionCreateRequest request)
    {
        await ValidateNames(request);
    }

    public async Task ValidateUpdateAsync(DimensionUpdateRequest request)
    {
        await ValidateNames(request, request.Id);
    }

    private async Task ValidateNames(ILocalizedNameDto dto, Guid? id = null)
    {
        var errors = new ErrorCollector();
        errors.Required(nameof(dto.NameFi), dto.NameFi, "Name (Finnish)", "Nimi (suomi)");
        errors.Required(nameof(dto.NameEn), dto.NameEn, "Name (English)", "Nimi (englanti)");
        errors.ThrowIfAny();

        var nameFi = dto.NameFi.Trim();
        var nameEn = dto.NameEn.Trim();

        var existing = await dimensionRepository.GetAllAsync();

        if (existing.Any(d => d.Id != id && string.Equals(d.NameFi, nameFi, StringComparison.OrdinalIgnoreCase)))
            errors.Add(ValidationErrors.Taken(nameof(dto.NameFi), "Name (Finnish)", "Nimi (suomi)"));

        if (existing.Any(d => d.Id != id && string.Equals(d.NameEn, nameEn, StringComparison.OrdinalIgnoreCase)))
            errors.Add(ValidationErrors.Taken(nameof(dto.NameEn), "Name (English)", "Nimi (englanti)"));

        errors.ThrowIfAny();
    }
}