using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using wema_Test_Backend_Engineer_Afeez.Services.Interface;

namespace wema_Test_Backend_Engineer_Afeez.Services.Implementation
{
    public class CacheService: ICacheService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public CacheService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("RedisConnection");
            _redis = ConnectionMultiplexer.Connect(connectionString + ",abortConnect=false");
            _database = _redis.GetDatabase();
        }

        // Add or update a cache entry
        public async Task AddOrUpdateAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            string serializedValue = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, serializedValue, expiry);
        }

        // Get a cache entry
        public async Task<T> GetAsync<T>(string key)
        {
            var serializedValue = await _database.StringGetAsync(key);
            if (serializedValue.IsNullOrEmpty)
            {
                return default(T);
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(serializedValue);
        }

        // Remove a cache entry
        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
