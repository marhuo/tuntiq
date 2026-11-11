namespace TuntiQ.Application.Dimensions.Messages;

public class DimensionUpdateRequest : ILocalizedNameDto
{
    public required Guid Id { get; init; }
    public required string NameEn { get; init; }
    public required string NameFi { get; init; }
    public required bool AllowMultiple { get; init; }
    public required bool IsMandatory { get; init; }
    public bool Enabled { get; private set; }
}
