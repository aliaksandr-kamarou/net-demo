using Confluent.Kafka;

namespace EMarketApi.Infrastructure.Kafka.Producer
{
    public interface IKafkaProducer<TValue>
    {
        Task SendMessage(string topicName, string messageKey, TValue messageValue, Headers? headers = default);
    }
}
