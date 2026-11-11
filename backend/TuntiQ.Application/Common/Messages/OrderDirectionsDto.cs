using TuntiQ.Domain.Common;

namespace TuntiQ.Application.Common.Messages;

public enum OrderDirectionsDto
{
    Asc,
    Desc
}

public static class OrderDirectionsDtoExtensions
{
    extension(OrderDirectionsDto dto)
    {
        public OrderDirections ToDomain() => dto switch
        {
            OrderDirectionsDto.Asc => OrderDirections.Asc,
            OrderDirectionsDto.Desc => OrderDirections.Desc,
            _ => throw new ArgumentOutOfRangeException(nameof(dto))
        };
    }
}
