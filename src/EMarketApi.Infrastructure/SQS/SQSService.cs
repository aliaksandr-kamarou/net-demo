using Amazon.SQS;
using Amazon.SQS.Model;
using EMarketApi.Infrastructure.Kafka;

namespace EMarketApi.Infrastructure.SQS
{
    public class SQSService : ISQSService
    {
        private readonly string sqsUrl;
        private readonly IAmazonSQS sqsClient;

        public SQSService(string sqsUrl, IAmazonSQS sqsClient)
        {
            this.sqsUrl = sqsUrl;
            this.sqsClient = sqsClient;
        }

        public async Task DeleteMessageAsync(string receiptHandle, CancellationToken cancellationToken)
        {
            var request = new DeleteMessageRequest(this.sqsUrl, receiptHandle);

            var response = await this.sqsClient.DeleteMessageAsync(request, cancellationToken);
            var responseCode = (int)response.HttpStatusCode;

            if (!(responseCode >= 200 && responseCode <= 299))
            {
                throw new Exception($"Publish to SQS failed. HttpCode {response.HttpStatusCode}");
            }
        }

        public async Task SendMessageAsync(string topic, string key, string value, int retryCount, string traceId, CancellationToken cancellationToken)
        {
            retryCount++;

            var attributes = new Dictionary<string, MessageAttributeValue>
            {
                { MessageAttributes.MessageKey, new MessageAttributeValue { DataType = "String", StringValue = key } },
                { MessageAttributes.Topic, new MessageAttributeValue { DataType = "String", StringValue = topic } },
                { MessageAttributes.RetryCount, new MessageAttributeValue { DataType = "String", StringValue = retryCount.ToString() } },
                { MessageAttributes.TraceID, new MessageAttributeValue { DataType = "String", StringValue = traceId, } }
            };

            var sqsRequest = new SendMessageRequest
            {
                QueueUrl = this.sqsUrl,
                MessageBody = value,
                MessageAttributes = attributes,
                DelaySeconds = 10,
            };

            var response = await this.sqsClient.SendMessageAsync(sqsRequest, cancellationToken);
            var responseCode = (int)response.HttpStatusCode;

            if (!(responseCode >= 200 && responseCode <= 299))
            {
                throw new Exception($"Publish to SQS failed. HttpCode {response.HttpStatusCode}");
            }
        }
    }
}
