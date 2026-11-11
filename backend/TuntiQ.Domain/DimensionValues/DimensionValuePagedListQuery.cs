namespace TuntiQ.Domain.DimensionValues;

public class DimensionValuePagedListQuery : PagedListQuery
{
    public bool? Enabled { get; set; }
    public string? Language { get; set; }
    public OrderDirections OrderDirection { get; set; } 
}
