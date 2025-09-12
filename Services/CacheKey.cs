namespace BuildCacheRedisProjectMini.Services
{
    public class CacheKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKey"/> class
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="prefixes">The prefixes</param>
        public CacheKey(string key, params string[] prefixes)
        {
            this.Key = key;
            this.Prefixes.AddRange(((IEnumerable<string>)prefixes).Where<string>((Func<string, bool>)(prefix => !string.IsNullOrEmpty(prefix))));
        }

        /// <summary>
        /// Creates the create cache key parameters
        /// </summary>
        /// <param name="createCacheKeyParameters">The create cache key parameters</param>
        /// <param name="keyObjects">The key objects</param>
        /// <returns>The cache key</returns>
        public virtual CacheKey Create(
          Func<object, object> createCacheKeyParameters,
          params object[] keyObjects)
        {
            CacheKey cacheKey = new CacheKey(this.Key, this.Prefixes.ToArray());
            if (!((IEnumerable<object>)keyObjects).Any<object>())
                return cacheKey;
            cacheKey.Key = string.Format(cacheKey.Key, ((IEnumerable<object>)keyObjects).Select<object, object>(createCacheKeyParameters).ToArray<object>());
            for (int index = 0; index < cacheKey.Prefixes.Count; ++index)
                cacheKey.Prefixes[index] = string.Format(cacheKey.Prefixes[index], ((IEnumerable<object>)keyObjects).Select<object, object>(createCacheKeyParameters).ToArray<object>());
            return cacheKey;
        }

        /// <summary>
        /// Gets or sets the value of the key
        /// </summary>
        public string Key { get; protected set; }

        /// <summary>
        /// Gets or sets the value of the prefixes
        /// </summary>
        public List<string> Prefixes { get; protected set; } = new List<string>();

        /// <summary>
        /// Gets or sets the value of the cache time
        /// </summary>
        ///  var redisConfiguration = builder.Configuration.GetSection("Redis")["ConnectionString"];
        /// <summary>
        /// Gets or sets the value of the cache time
        /// </summary>
        public int CacheTime { get; set; } = 60;
    }
}
