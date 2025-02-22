using Confluent.Kafka;
using EMarketApi.Infrastructure.Kafka.Helpers;
using EMarketApi.Infrastructure.SNS;
using EMarketApi.Infrastructure.SQS;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using EMarketApi.Common;
using System.Diagnostics;
using System.Text.Json;
using EMarketApi.Infrastructure.Kafka.Extensions;

namespace EMarketApi.Infrastructure.Kafka.Consumer
{
    public class KafkaConsumer<TValue> : IKafkaConsumer<TValue>
        where TValue : class, IRequest, new()
    {
        private readonly IMediator mediator;
        private readonly IConsumer<string, TValue> consumer;
        private readonly ISNSService snsService;
        private readonly ISQSService sqsService;
        private readonly IOptions<KafkaOptions<TValue>> options;
        private readonly ILogger<KafkaConsumer<TValue>> logger;
        private readonly IServiceScopeFactory scopeFactory;

        public KafkaConsumer(IMediator mediator, ISNSService snsService, ISQSService sqsService, ILogger<KafkaConsumer<TValue>> logger, IOptions<KafkaOptions<TValue>> options, IServiceScopeFactory scopeFactory)
        {
            this.mediator = mediator;
            this.snsService = snsService;
            this.sqsService = sqsService;
            this.logger = logger;
            this.options = options;
            this.scopeFactory = scopeFactory;

            var config = new ConsumerConfig
            {
                BootstrapServers = this.options.Value.BootstrapServers,
                GroupId = this.options.Value.Group,
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnableAutoCommit = true,
                EnableAutoOffsetStore = false,
            };

            this.consumer = new ConsumerBuilder<string, TValue>(config)
                .SetValueDeserializer(new KafkaProtobufDeserializer<TValue>())
                .Build();
        }

        public async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            this.consumer.Subscribe(this.options.Value.Topic);

            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = this.consumer.Consume(cancellationToken);
                    var message = consumeResult.Message.Value;
                    var topic = consumeResult.Topic;
                    var headers = consumeResult.Message.Headers?.ToDictionary(h => h.Key, h => Encoding.UTF8.GetString(h.GetValueBytes()));

                    using var activity = Telemetry.Activity?.StartActivity("Consuming message", ActivityKind.Consumer, parentId: headers?.FirstOrDefault(c => c.Key == MessageAttributes.TraceID).Value)
                            .SetTag(nameof(consumeResult.Topic), consumeResult.Topic);

                    if (consumeResult.IsPartitionEOF)
                    {
                        continue;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        activity?.AddEvent(new ActivityEvent($"Stop consuming for {consumeResult.Topic}"));
                        break;
                    }

                    var key = consumeResult.Message.Key;
                    var value = consumeResult.Message.Value;

                    activity?.SetTag("MessageKey", JsonSerializer.Serialize(key));
                    activity?.SetTag("MessageValue", JsonSerializer.Serialize(value));

                    try
                    {
                        await this.mediator.Send(value, cancellationToken);

                        activity?.SetStatus(ActivityStatusCode.Ok);
                    }
                    catch (Exception ex)
                    {
                        activity?.AddEvent(new ActivityEvent("Error while message processing"));
                        activity?.SetTag(nameof(Exception.StackTrace), ex.StackTrace);
                    }

                    this.consumer.StoreOffset(consumeResult);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error while consuming");
                }
            }
        }

        private async Task HandleFailure(string messageKey, TValue messageValue, Headers headers, string topic, CancellationToken cancellationToken)
        {
            using var activity = Telemetry.Activity.StartActivity("Retry handling");

            var retryCount = headers.GetRetryCount();
            var traceId = headers.FirstOrDefault(c => c.Key == MessageAttributes.TraceID);

            activity.AddTag(nameof(MessageAttributes.RetryCount), retryCount);

            if (this.options.Value.MaxRetryCount > retryCount && !string.IsNullOrWhiteSpace(messageKey?.ToString()))
            {
                //await this.sqsService.SendRetryMessageAsync(
                //    topic,
                //    messageKey.ToString(),
                //    JsonSerializer.Serialize(messageValue),
                //    retryCount,
                //    cancellationToken);

                activity?.AddEvent(new ActivityEvent("Retry message was sent"));
                activity?.SetStatus(ActivityStatusCode.Ok);

                return;
            }

            var messageValueJson = JsonDocument.Parse(JsonSerializer.Serialize(messageValue));

            var snsHeaders = new Headers
            {
                { MessageAttributes.RetryCount, Encoding.UTF8.GetBytes(retryCount.ToString()) },
            };

            // await this.snsService.SendAlertAsync(this.options.Value.DLQTopic, messageKey.ToString(), messageValueJson, headers);

            activity?.SetStatus(ActivityStatusCode.Error, "Notification was sent to alerting system");
        }
    }
}
