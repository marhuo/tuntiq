namespace TuntiQ.Application.Dimensions.Messages;

public class DimensionCreateRequest : ILocalizedNameDto
{
    public required DimensionDtoKinds Kind { get; init; }
    public required string NameEn { get; init; }
    public required string NameFi { get; init; }
    public required bool AllowMultiple { get; init; }
    public required bool IsMandatory { get; init; }
}
