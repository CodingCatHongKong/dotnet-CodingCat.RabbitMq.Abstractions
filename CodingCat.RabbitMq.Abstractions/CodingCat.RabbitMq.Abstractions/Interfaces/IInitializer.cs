using RabbitMQ.Client;
using System.Collections.Generic;
using IMqSubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface IInitializer
    {
        IEnumerable<ISubscriberFactory> Factories { get; }

        IInitializer Configure(IModel channel);

        IMqSubscriber[] GetSubscribed(IConnection connection);
    }
}