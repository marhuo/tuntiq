using TuntiQ.Domain.Dimensions;

namespace TuntiQ.Application.Dimensions.Messages;

public class DimensionValueDto
{
    public required Guid Id { get; init; }
    public required Guid DimensionId { get; init; }
    public required Guid? ReferenceId { get; init; }
    public required string NameEn { get; init; }
    public required string NameFi { get; init; }
    public required bool Enabled { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }

    public static DimensionValueDto ToDto(DimensionValue value) => new()
    {
        Id = value.Id,
        DimensionId = value.DimensionId,
        ReferenceId = value.ReferenceId,
        NameEn = value.NameEn,
        NameFi = value.NameFi,
        Enabled = value.Enabled,
        CreatedAt = value.CreatedAt
    };
}
