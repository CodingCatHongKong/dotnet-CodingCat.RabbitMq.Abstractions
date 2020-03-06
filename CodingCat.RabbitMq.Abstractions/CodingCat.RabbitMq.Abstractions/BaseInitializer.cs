using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;
using ISubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class BaseInitializer : IInitializer
    {
        public IEnumerable<ISubscriberFactory> Factories { get; }
        public IEnumerable<IExchange> Exchanges { get; }
        public IEnumerable<IQueue> Queues { get; }

        #region Constructor(s)

        public BaseInitializer(
            IModel channel,
            IEnumerable<ISubscriberFactory> factories,
            IEnumerable<IExchange> exchanges,
            IEnumerable<IQueue> queues
        )
        {
            this.Factories = factories;
            this.Exchanges = exchanges;
            this.Queues = queues;

            using (channel)
            {
                foreach (var exchange in this.Exchanges)
                    exchange.Declare(channel);

                foreach (var queue in this.Queues)
                    queue.Declare(channel);

                channel.Close();
            }
        }

        #endregion Constructor(s)

        public abstract IInitializer Configure(IModel channel);

        public virtual ISubscriber[] GetSubscribed(IConnection connection)
        {
            return this.Factories
                .ToArray()
                .Select(factory =>
                    factory.GetSubscribed(
                        connection.CreateModel()
                    )
                )
                .ToArray();
        }
    }
}