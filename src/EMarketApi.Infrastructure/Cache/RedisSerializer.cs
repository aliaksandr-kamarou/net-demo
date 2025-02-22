using ProtoBuf;

namespace EMarketApi.Infrastructure.Cache
{
    public class RedisSerializer : IRedisSerializer
    {
        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == default)
            {
                return default;
            }

            using var memoryStream = new MemoryStream(bytes);
            return Serializer.Deserialize<T>(memoryStream);
        }

        public byte[] Serialize(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, obj);
            return memoryStream.ToArray();
        }
    }
}
