using RabbitMQ.Client;
using IBaseSubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface ISubscriberFactory
    {
        IBaseSubscriber GetSubscribed(IModel channel);
    }
}