namespace TuntiQ.Application.Dimensions.Messages;

public class DimensionValuePagedListRequest : PagedListRequest
{
    /// <summary>
    /// Limits the result to one dimension. Null searches across all dimensions.
    /// </summary>
    public Guid? DimensionId { get; set; }
    public bool? Enabled { get; set; }

    /// <summary>
    /// Language used for sorting by name: "fi" (default) or "en".
    /// </summary>
    public string? Language { get; set; }

    public OrderDirectionsDto OrderDirection { get; set; }
}
