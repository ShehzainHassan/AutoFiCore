namespace AutoFiCore.Utilities
{
    /// <summary>
    /// Represents a paginated result set for queries that return multiple items.
    /// </summary>
    /// <typeparam name="T">The type of items in the result set.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// The list of items returned for the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// The current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int PageSize { get; set; }
    }
}