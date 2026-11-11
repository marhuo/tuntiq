namespace TuntiQ.Domain.Common;


public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
    public PagedList(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}

public static class PagedListExtensions
{
    extension<T>(IQueryable<T> source)
    {
        public PagedList<T> ToPagedList(int pageNumber, int pageSize)
        {
            (pageNumber, pageSize) = Normalize(pageNumber, pageSize);
            var totalCount = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, pageNumber, pageSize, totalCount);
        }
    }

    extension<T>(IEnumerable<T> source)
    {
        public PagedList<T> ToPagedList(int pageNumber, int pageSize)
        {
            (pageNumber, pageSize) = Normalize(pageNumber, pageSize);
            var list = source as IReadOnlyCollection<T> ?? source.ToList();
            var items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PagedList<T>(items, pageNumber, pageSize, list.Count);
        }
    }

    private static (int Page, int PageSize) Normalize(int page, int pageSize) =>
        (Math.Max(page, 1), Math.Clamp(pageSize, 1, 100));
}