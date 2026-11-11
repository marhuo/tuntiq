namespace TuntiQ.Application.Dimensions.Messages;

public enum DimensionDtoKinds
{
    General,
    Project,
    Ticket
}
public static class DimensionDtoExtensions
{
    extension(DimensionDtoKinds dto)
    {
        public DimensionKinds ToDomain() => dto switch
        {
            DimensionDtoKinds.General => DimensionKinds.General,
            DimensionDtoKinds.Project => DimensionKinds.Project,
            DimensionDtoKinds.Ticket => DimensionKinds.Ticket,
            _ => throw new ArgumentOutOfRangeException(nameof(dto))
        };
    }

    extension(DimensionKinds kind)
    {
        public DimensionDtoKinds ToDto() => kind switch
        {
            DimensionKinds.General => DimensionDtoKinds.General,
            DimensionKinds.Project => DimensionDtoKinds.Project,
            DimensionKinds.Ticket => DimensionDtoKinds.Ticket,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }
}