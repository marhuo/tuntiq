namespace TuntiQ.Application.Dimensions.Messages;

public class DimensionValueCreateRequest : ILocalizedNameDto
{
    public required Guid DimensionId { get; init; }
    public required string NameEn { get; init; }
    public required string NameFi { get; init; }

    /// <summary>
    /// Id of the referenced project or ticket. Required for Project and Ticket dimensions, must be null for General.
    /// </summary>
    public Guid? ReferenceId { get; init; }
}
