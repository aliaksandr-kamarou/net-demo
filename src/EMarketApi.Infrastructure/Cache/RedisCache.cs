using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EMarketApi.Infrastructure.Cache
{
    public class RedisCache : IRedisCache
    {
        private readonly TimeSpan defaultExpiration;
        private readonly IConnectionMultiplexer connection;
        private readonly IRedisSerializer serializer;
        private readonly IDatabase database;

        public RedisCache(IOptions<RedisOptions> redisOptions, IConnectionMultiplexer connection, IRedisSerializer serializer)
        {
            this.defaultExpiration = TimeSpan.FromMinutes(redisOptions.Value.DefaultExpiration);
            this.connection = connection;
            this.serializer = serializer;
            this.database = this.connection.GetDatabase();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var stringValue = await this.database.StringGetAsync(key);
            if (stringValue == RedisValue.Null)
            {
                return default;
            }

            return this.serializer.Deserialize<T>((byte[])stringValue);
        }

        public async Task<bool> SetAsync<T>(string key, T value)
        {
            return await this.database.StringSetAsync(key, this.serializer.Serialize(value), this.defaultExpiration);
        }
    }
}
