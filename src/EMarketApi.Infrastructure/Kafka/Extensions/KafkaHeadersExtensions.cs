using Confluent.Kafka;
using System.Text;

namespace EMarketApi.Infrastructure.Kafka.Extensions
{
    public static class KafkaHeadersExtensions
    {
        public static int GetRetryCount(this Headers headers)
        {
            var retryCount = headers?.FirstOrDefault(header => header.Key == MessageAttributes.RetryCount);
            
            if (retryCount == null)
            {
                return 0;
            }

            var bytes = retryCount.GetValueBytes();
            var retryCountString = Encoding.UTF8.GetString(bytes);
            
            if (int.TryParse(retryCountString, out var retriesCount))
            {
                return retriesCount;
            }

            return 0;
        }
    }
}
