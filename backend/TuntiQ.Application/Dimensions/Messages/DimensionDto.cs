namespace TuntiQ.Application.Dimensions.Messages;

public class DimensionDto
{
    public required Guid Id { get; init; }
    public required string NameEn { get; init; }
    public required string NameFi { get; init; }
    public required DimensionDtoKinds Kind { get; init; }
    public required bool AllowMultiple { get; init; }
    public required bool IsMandatory { get; init; }
    public required bool Enabled { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public static DimensionDto ToDto(Dimension dimension) => new()
    {
        Id = dimension.Id,
        NameEn = dimension.NameEn,
        NameFi = dimension.NameFi,
        Kind = dimension.Kind.ToDto(),
        AllowMultiple = dimension.AllowMultiple,
        IsMandatory = dimension.IsMandatory,
        Enabled = dimension.Enabled,
        CreatedAt = dimension.CreatedAt
    };

}
