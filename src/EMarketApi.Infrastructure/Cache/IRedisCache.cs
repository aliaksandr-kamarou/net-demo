namespace EMarketApi.Infrastructure.Cache
{
    public interface IRedisCache
    {
        Task<bool> SetAsync<T>(string key, T value);

        Task<T> GetAsync<T>(string key);
    }
}
