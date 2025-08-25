namespace VHouse.Classes
{
    /// <summary>
    /// Represents a paginated result set with metadata.
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// The current page of data.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        /// <summary>
        /// Whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Whether there is a next page.
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Index of first item on current page.
        /// </summary>
        public int FirstItemIndex => (CurrentPage - 1) * PageSize + 1;

        /// <summary>
        /// Index of last item on current page.
        /// </summary>
        public int LastItemIndex => Math.Min(CurrentPage * PageSize, TotalItems);
    }

    /// <summary>
    /// Pagination parameters for requests.
    /// </summary>
    public class PaginationParameters
    {
        private int _pageSize = 20;
        private const int MaxPageSize = 100;

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        /// <summary>
        /// Search term for filtering results.
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Sort field name.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc/desc).
        /// </summary>
        public string SortDirection { get; set; } = "asc";

        /// <summary>
        /// Calculate skip count for database queries.
        /// </summary>
        public int Skip => (Page - 1) * PageSize;

        /// <summary>
        /// Take count for database queries.
        /// </summary>
        public int Take => PageSize;
    }
}