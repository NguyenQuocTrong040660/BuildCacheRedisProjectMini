
using StackExchange.Redis;
using Nito.AsyncEx;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Text.Json;

namespace BuildCacheRedisProjectMini.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _distributedCache;


        private static readonly List<string> _keys;

        private static readonly AsyncLock _locker = new AsyncLock();

        /// <summary>
        /// contructor
        /// </summary>
        static RedisCacheService() => _keys = new List<string>();

        public RedisCacheService( IConnectionMultiplexer redis, IDatabase database)
        { 
           
            _redis = redis;
            _distributedCache = database;
        }

        public async Task SetCacheValueAsync<T>(string key, T value)
        {
            var db = _redis.GetDatabase();
            var json = System.Text.Json.JsonSerializer.Serialize<T>(value);
            await db.StringSetAsync(key.ToString(), json);
        }

        public async Task<T> GetCacheValueAsync<T>(string key)
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            return value.HasValue ? System.Text.Json.JsonSerializer.Deserialize<T>(value) : default;
        }

        /// <summary>
        ///  custom
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        async Task<(bool isSet, T item)> TryGetItem<T>(CacheKey key)
        {
            RedisValue redisValue = await _distributedCache.StringGetAsync((RedisKey)key.Key);
            if (string.IsNullOrEmpty(redisValue))
                return (false, default(T));
            T item = JsonConvert.DeserializeObject<T>(redisValue);
            return (true, item);
        }

    
        /// <summary>
        ///  custom
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task Set(CacheKey key, object data)
        {

            if ((key?.CacheTime ?? 0) <= 0 || data == null)
                return;

            using (await _locker.LockAsync())
            {
                await _distributedCache.StringSetAsync((RedisKey)key.Key, JsonConvert.SerializeObject(data));
                _keys.Add(key.Key);
            }
        }

      

        public void Dispose()
        {
            
        }

        public Task<T> Get<T>(CacheKey key, Func<Task<T>> acquire)
        {
            throw new NotImplementedException();
        }

        public Task<T> Get<T>(CacheKey key, Func<T> acquire)
        {
            throw new NotImplementedException();
        }

        public Task<T> Get<T>(CacheKey key, T defaultValue)
        {
            throw new NotImplementedException();
        }

        public Task<object> Get(CacheKey key)
        {
            throw new NotImplementedException();
        }

        public T GetFieldHash<T>(string key, string field) where T : class
        {
            throw new NotImplementedException();
        }

        public CacheKey PrepareKey(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            throw new NotImplementedException();
        }

        public CacheKey PrepareKeyForDefaultCache(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            throw new NotImplementedException();
        }

        public Task Remove(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            throw new NotImplementedException();
        }

        public Task RemoveByPrefix(string prefix, params object[] prefixParameters)
        {
            throw new NotImplementedException();
        }

        public Task RemoveHash(string key)
        {
            throw new NotImplementedException();
        }

       

        public void SetFieldsHash(string key, HashEntry hashData, int cacheTime = 0)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Clear()
        {
            using (await _locker.LockAsync())
            {
                foreach (string key in _keys)
                {
                    int num = await this._distributedCache.KeyDeleteAsync((RedisKey)key) ? 1 : 0;
                }
                _keys.Clear();
            }
        }


     
    }
}
