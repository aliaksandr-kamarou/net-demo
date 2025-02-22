using Confluent.Kafka;
using EMarketApi.Common;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EMarketApi.Infrastructure.Kafka.Producer
{
    public class KafkaProducer<TValue> : IKafkaProducer<TValue>
    {
        private readonly IProducer<string, TValue> producer;
        private readonly ILogger<KafkaProducer<TValue>> logger;

        public KafkaProducer(IProducer<string, TValue> producer, ILogger<KafkaProducer<TValue>> logger)
        {
            this.producer = producer;
            this.logger = logger;
        }

        public async Task SendMessage(string topicName, string messageKey, TValue messageValue, Headers? headers = null)
        {
            using var activity = Telemetry.Activity.StartActivity("Send message to Kafka", ActivityKind.Producer);

            if (string.IsNullOrEmpty(topicName))
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Topic name is null or empty.");

                throw new ArgumentNullException(nameof(topicName));
            }

            try
            {
                var messageToPublish = new Message<string, TValue>
                {
                    Key = messageKey,
                    Value = messageValue,
                    Headers = headers,
                };

                await this.producer.ProduceAsync(topicName, messageToPublish);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (ProduceException<string, TValue> exception)
            {
                activity?.SetTag("StackTrace", exception.StackTrace);
                activity?.SetStatus(ActivityStatusCode.Error, exception.StackTrace);
                this.logger.LogError(exception, "kafka Producer");

                throw;
            }
        }
    }
}
