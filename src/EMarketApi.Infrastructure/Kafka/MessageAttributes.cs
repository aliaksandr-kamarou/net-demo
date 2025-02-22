namespace EMarketApi.Infrastructure.Kafka
{
    public class MessageAttributes
    {
        /// <summary>
        /// Gets topic.
        /// </summary>
        public const string Topic = "Topic";

        /// <summary>
        /// Gets topic message key.
        /// </summary>
        public const string MessageKey = "Key";

        /// <summary>
        /// Gets MaxRetryCount.
        /// </summary>
        public const string RetryCount = "RetryCount";

        /// <summary>
        /// Gets message error.
        /// </summary>
        public const string Error = "Error";

        /// <summary>
        /// Gets TraceId.
        /// </summary>
        public const string TraceID = "TraceID";
    }
}
