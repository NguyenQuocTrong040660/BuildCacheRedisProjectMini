using StackExchange.Redis;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BuildCacheRedisProjectMini.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _distributedCache;

        public RedisCacheService(IConnectionMultiplexer redis, IDatabase database)
        {
            _redis = redis;
            _distributedCache = database;
        }

        public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            await _distributedCache.StringSetAsync(key, json, expiry);
        }

        public async Task<T?> GetCacheValueAsync<T>(string? key)
        {
            if (string.IsNullOrEmpty(key))
                return default;

            var value = await _distributedCache.StringGetAsync(key);
            if (!value.HasValue)
                return default;

            return System.Text.Json.JsonSerializer.Deserialize<T>(value.ToString());
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return await _distributedCache.KeyExistsAsync(key);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            return await _distributedCache.KeyDeleteAsync(key);
        }

        async Task<(bool isSet, T? item)> TryGetItem<T>(CacheKey key)
        {
            RedisValue redisValue = await _distributedCache.StringGetAsync((RedisKey)key.Key);
            if (!redisValue.HasValue)
                return (false, default);
            
            T? item = JsonConvert.DeserializeObject<T>(redisValue.ToString());
            return (true, item);
        }

        public async Task Set(CacheKey key, object data)
        {
            if (key == null || key.CacheTime <= 0 || data == null)
                return;

            var expiry = TimeSpan.FromMinutes(key.CacheTime);
            await _distributedCache.StringSetAsync((RedisKey)key.Key, JsonConvert.SerializeObject(data), expiry);
        }

        public async Task<T> Get<T>(CacheKey key, Func<Task<T>> acquire)
        {
            var (isSet, item) = await TryGetItem<T>(key);
            if (isSet && item != null)
                return item;

            T result = await acquire();
            if (result != null)
            {
                await Set(key, result);
            }
            return result;
        }

        public async Task<T> Get<T>(CacheKey key, Func<T> acquire)
        {
            var (isSet, item) = await TryGetItem<T>(key);
            if (isSet && item != null)
                return item;

            T result = acquire();
            if (result != null)
            {
                await Set(key, result);
            }
            return result;
        }

        public async Task<T> Get<T>(CacheKey key, T defaultValue)
        {
            var (isSet, item) = await TryGetItem<T>(key);
            if (isSet && item != null)
                return item;

            return defaultValue;
        }

        public async Task<object?> Get(CacheKey key)
        {
            RedisValue redisValue = await _distributedCache.StringGetAsync((RedisKey)key.Key);
            if (!redisValue.HasValue)
                return null;
            return JsonConvert.DeserializeObject(redisValue.ToString());
        }

        public T? GetFieldHash<T>(string key, string field) where T : class
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(field))
                return null;

            var redisValue = _distributedCache.HashGet(key, field);
            if (!redisValue.HasValue)
                return null;

            return JsonConvert.DeserializeObject<T>(redisValue.ToString());
        }

        public CacheKey PrepareKey(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            return cacheKey.Create(x => x, cacheKeyParameters);
        }

        public CacheKey PrepareKeyForDefaultCache(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            return PrepareKey(cacheKey, cacheKeyParameters);
        }

        public async Task Remove(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            var preparedKey = PrepareKey(cacheKey, cacheKeyParameters);
            await _distributedCache.KeyDeleteAsync(preparedKey.Key);
        }

        public async Task RemoveByPrefix(string prefix, params object[] prefixParameters)
        {
            var formattedPrefix = prefix;
            if (prefixParameters != null && prefixParameters.Length > 0)
            {
                formattedPrefix = string.Format(prefix, prefixParameters);
            }
            var pattern = formattedPrefix + "*";

            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(_distributedCache.Database, pattern).ToArray();
                if (keys.Length > 0)
                {
                    await _distributedCache.KeyDeleteAsync(keys);
                }
            }
        }

        public async Task RemoveHash(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            await _distributedCache.KeyDeleteAsync(key);
        }

        public void SetFieldsHash(string key, HashEntry hashData, int cacheTime = 0)
        {
            if (string.IsNullOrEmpty(key))
                return;

            _distributedCache.HashSet(key, new[] { hashData });
            if (cacheTime > 0)
            {
                _distributedCache.KeyExpire(key, TimeSpan.FromMinutes(cacheTime));
            }
        }

        public async Task Clear()
        {
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(_distributedCache.Database).ToArray();
                if (keys.Length > 0)
                {
                    await _distributedCache.KeyDeleteAsync(keys);
                }
            }
        }

        public Task<List<string>> GetAllKeysAsync()
        {
            var keysList = new List<string>();
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                var keys = server.Keys(_distributedCache.Database);
                foreach (var key in keys)
                {
                    keysList.Add(key.ToString());
                }
            }
            return Task.FromResult(keysList.Distinct().ToList());
        }

        public void Dispose()
        {
            // Multiplexer is registered as Singleton, so we don't dispose it here.
        }
    }
}
