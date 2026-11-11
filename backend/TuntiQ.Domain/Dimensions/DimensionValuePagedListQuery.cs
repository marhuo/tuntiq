namespace TuntiQ.Domain.Dimensions;

public class DimensionValuePagedListQuery : PagedListQuery
{
    /// <summary>
    /// Limits the result to one dimension. Null searches across all dimensions.
    /// </summary>
    public Guid? DimensionId { get; set; }
    public bool? Enabled { get; set; }
    public string? Language { get; set; }
    public OrderDirections OrderDirection { get; set; } 
}
