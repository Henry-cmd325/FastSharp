using System;
using System.Collections.Generic;

namespace FastSharp.Models
{
    /// <summary>
    /// Represents a paginated result set returned by list endpoints.
    /// </summary>
    /// <typeparam name="T">The type of items in the result.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>Gets or sets the current page number (1-based).</summary>
        public int Page { get; set; }

        /// <summary>Gets or sets the maximum number of items per page.</summary>
        public int PageSize { get; set; }

        /// <summary>Gets or sets the total number of items across all pages.</summary>
        public int TotalItems { get; set; }

        /// <summary>Gets or sets the items on the current page.</summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>Gets the total number of pages calculated from <see cref="TotalItems"/> and <see cref="PageSize"/>.</summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        /// <summary>
        /// Initializes a new instance of <see cref="PagedResult{T}"/>.
        /// </summary>
        /// <param name="items">The items on the current page.</param>
        /// <param name="totalItems">The total number of items across all pages.</param>
        /// <param name="page">The current page number (1-based).</param>
        /// <param name="pageSize">The maximum number of items per page.</param>
        public PagedResult(IEnumerable<T> items, int totalItems, int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
            TotalItems = totalItems;
            Items = items;
        }
    }
}
