namespace EMarketApi.Infrastructure.SQS
{
    public interface ISQSService
    {
        Task SendMessageAsync(string topic, string key, string value, int retryCount, string traceId, CancellationToken cancellationToken);

        Task DeleteMessageAsync(string receiptHandle, CancellationToken cancellationToken);
    }
}
