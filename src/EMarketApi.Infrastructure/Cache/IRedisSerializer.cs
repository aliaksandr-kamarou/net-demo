namespace EMarketApi.Infrastructure.Cache
{
    public interface IRedisSerializer
    {
        byte[] Serialize(object obj);

        T Deserialize<T>(byte[] bytes);
    }
}
