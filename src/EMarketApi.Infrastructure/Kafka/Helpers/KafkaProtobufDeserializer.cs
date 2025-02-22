using Confluent.Kafka;
using ProtoBuf;
using SerializationContext = Confluent.Kafka.SerializationContext;

namespace EMarketApi.Infrastructure.Kafka.Helpers
{
    public class KafkaProtobufDeserializer<TSource> :
        IDeserializer<TSource>
        where TSource : new()
    {
        public TSource Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull || data.IsEmpty)
            {
                return new TSource();
            }

            return Serializer.Deserialize<TSource>(data);
        }
    }
}
