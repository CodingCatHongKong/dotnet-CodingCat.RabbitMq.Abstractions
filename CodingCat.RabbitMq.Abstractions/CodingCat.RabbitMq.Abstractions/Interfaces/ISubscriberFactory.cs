using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface ISubscriberFactory
    {
        ISubscriber GetSubscribed(IModel channel);
    }
}