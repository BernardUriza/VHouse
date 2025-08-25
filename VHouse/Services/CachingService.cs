using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using VHouse.Interfaces;

namespace VHouse.Services
{
    /// <summary>
    /// Distributed caching service implementation using Redis or in-memory cache.
    /// </summary>
    public class CachingService : ICachingService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachingService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public CachingService(IDistributedCache cache, ILogger<CachingService> logger)
        {
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key);
                if (cachedValue == null)
                    return null;

                return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var options = new DistributedCacheEntryOptions();
                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }
                else
                {
                    // Default expiration of 30 minutes
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                }

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                await _cache.SetStringAsync(key, serializedValue, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
            }
        }

        public async Task RemovePatternAsync(string pattern)
        {
            try
            {
                // Note: This is a basic implementation. In production, you might want to use Redis-specific features
                // for pattern-based removal or maintain a registry of cache keys.
                _logger.LogWarning("RemovePatternAsync not fully implemented for distributed cache. Key pattern: {Pattern}", pattern);
                
                // For now, we'll just log the operation. In a real implementation with Redis,
                // you would use Redis SCAN and DEL commands for pattern matching.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached values for pattern: {Pattern}", pattern);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            await RemovePatternAsync(pattern);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);
                return value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if cached key exists: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var cachedValue = await GetAsync<T>(key);
                if (cachedValue != null)
                {
                    return cachedValue;
                }

                var item = await getItem();
                if (item != null)
                {
                    await SetAsync(key, item, expiration);
                }

                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOrSetAsync for key: {Key}", key);
                return await getItem();
            }
        }
    }
}