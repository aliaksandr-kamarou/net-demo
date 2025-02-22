namespace EMarketApi.Infrastructure.Kafka.Consumer
{
    public class KafkaOptions<TValue>
    {
        public string BootstrapServers { get; set; }

        public string Topic { get; set; }

        public string Group { get; set; }

        public string DLQTopic { get; set; }

        public int MaxRetryCount { get; set; }
    }
}
