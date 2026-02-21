namespace FastSharp.Models
{
    public class PagedResult<T>(IEnumerable<T> items, int totalItems, int page, int pageSize)
    {
        public int Page { get; set; } = page;
        public int PageSize { get; set; } = pageSize;
        public int TotalItems { get; set; } = totalItems;
        public IEnumerable<T> Items { get; set; } = items;
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }
}
