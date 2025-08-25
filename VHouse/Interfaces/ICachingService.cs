namespace VHouse.Interfaces
{
    /// <summary>
    /// Service interface for distributed caching operations.
    /// </summary>
    public interface ICachingService
    {
        /// <summary>
        /// Gets a cached value by key.
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Sets a value in cache with expiration.
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Removes a value from cache by key.
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes multiple values from cache by pattern.
        /// </summary>
        Task RemovePatternAsync(string pattern);

        /// <summary>
        /// Removes multiple values from cache by pattern (alias).
        /// </summary>
        Task RemoveByPatternAsync(string pattern);

        /// <summary>
        /// Checks if a key exists in cache.
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Gets or sets a cached value, computing it if not present.
        /// </summary>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiration = null) where T : class;
    }
}