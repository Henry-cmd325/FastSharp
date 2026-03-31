using System;
using System.Collections.Generic;

namespace FastSharp.Models
{
    public class PagedResult<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public IEnumerable<T> Items { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public PagedResult(IEnumerable<T> items, int totalItems, int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
            TotalItems = totalItems;
            Items = items;
        }
    }
}
