using MediatR;

namespace EMarketApi.Infrastructure.Kafka.Consumer
{
    public interface IKafkaConsumer<TValue> where TValue : class, IRequest, new()
    {
        Task ConsumeAsync(CancellationToken cancellationToken);
    }
}
