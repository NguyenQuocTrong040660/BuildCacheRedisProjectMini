using StackExchange.Redis;

namespace BuildCacheRedisProjectMini.Services
{
    public interface IRedisCacheService : IDisposable
    {
        /// <summary>
        /// Gets the key
        /// </summary>
        /// <typeparam name="T">The </typeparam>
        /// <param name="key">The key</param>
        /// <param name="acquire">The acquire</param>
        /// <returns>A task containing the</returns>
        Task<T> Get<T>(CacheKey key, Func<Task<T>> acquire);


        Task SetCacheValueAsync<T>(string key, T value);

        Task<T> GetCacheValueAsync<T>(string? key);
       

        /// <summary>
        /// Gets the key
        /// </summary>
        /// <typeparam name="T">The </typeparam>
        /// <param name="key">The key</param>
        /// <param name="acquire">The acquire</param>
        /// <returns>A task containing the</returns>
        Task<T> Get<T>(CacheKey key, Func<T> acquire);

        /// <summary>
        /// Gets the key
        /// </summary>
        /// <typeparam name="T">The </typeparam>
        /// <param name="key">The key</param>
        /// <param name="defaultValue">The default value</param>
        /// <returns>A task containing the</returns>
        Task<T> Get<T>(CacheKey key, T defaultValue);

        /// <summary>
        /// Gets the key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>A task containing the object</returns>
        Task<object> Get(CacheKey key);

        /// <summary>
        /// Removes the cache key
        /// </summary>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="cacheKeyParameters">The cache key parameters</param>
        Task Remove(CacheKey cacheKey, params object[] cacheKeyParameters);

        /// <summary>
        /// Sets the key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="data">The data</param>
        Task Set(CacheKey key, object data);

        /// <summary>
        /// Removes the by prefix using the specified prefix
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <param name="prefixParameters">The prefix parameters</param>
        Task RemoveByPrefix(string prefix, params object[] prefixParameters);

        /// <summary>
        /// Clears this instance
        /// </summary>
        Task Clear();

        /// <summary>
        /// Prepares the key using the specified cache key
        /// </summary>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="cacheKeyParameters">The cache key parameters</param>
        /// <returns>The cache key</returns>
        CacheKey PrepareKey(CacheKey cacheKey, params object[] cacheKeyParameters);

        /// <summary>
        /// Prepares the key for default cache using the specified cache key
        /// </summary>
        /// <param name="cacheKey">The cache key</param>
        /// <param name="cacheKeyParameters">The cache key parameters</param>
        /// <returns>The cache key</returns>
        CacheKey PrepareKeyForDefaultCache(CacheKey cacheKey, params object[] cacheKeyParameters);


        void SetFieldsHash(string key, HashEntry hashData, int cacheTime = 0);


        T GetFieldHash<T>(string key, string field) where T : class;

        Task RemoveHash(string key);

    }
}
