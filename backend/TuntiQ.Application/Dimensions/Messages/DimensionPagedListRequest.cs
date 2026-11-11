namespace TuntiQ.Application.Dimensions.Messages;

public class DimensionPagedListRequest : PagedListRequest
{
    public DimensionDtoOrderBy OrderBy { get; set; }

}
