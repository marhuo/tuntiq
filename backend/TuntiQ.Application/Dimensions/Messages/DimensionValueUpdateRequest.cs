namespace TuntiQ.Application.Dimensions.Messages;

/// <summary>
/// A dimension value cannot move to another dimension, so the dimension id is not part of the update.
/// </summary>
public class DimensionValueUpdateRequest : ILocalizedNameDto
{
    public required Guid Id { get; init; }
    public required string NameEn { get; init; }
    public required string NameFi { get; init; }
    public required bool Enabled { get; init; }
}
