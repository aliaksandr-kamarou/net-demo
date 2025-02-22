using Confluent.Kafka;

namespace EMarketApi.Infrastructure.Kafka.Helpers
{
    public class KafkaProtobufSerializer<T> : ISerializer<T> where T : class
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            using var ms = new MemoryStream();
            ProtoBuf.Serializer.Serialize<T>(ms, data);
            return ms.ToArray();
        }
    }
}
