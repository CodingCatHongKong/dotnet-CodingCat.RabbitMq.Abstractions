using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using CodingCat.Serializers.Impls;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CodingCat.RabbitMq.Abstractions.Tests.Abstracts
{
    public abstract class BaseTest : IDisposable
    {
        public IConnection UsingConnection { get; }
        public List<Exchange> DeclaredExchanges { get; } = new List<Exchange>();
        public List<Queue> DeclaredQueues { get; } = new List<Queue>();

        protected abstract IEnumerable<Exchange> DeclareExchanges();

        protected abstract IEnumerable<Queue> DeclareQueues();

        #region Constructor(s)

        public BaseTest()
        {
            this.UsingConnection = new ConnectionFactory()
            {
                Uri = new Uri(Constants.USING_RABBITMQ)
            }.CreateConnection();

            this.DeclaredExchanges.AddRange(this.DeclareExchanges());
            this.DeclaredQueues.AddRange(this.DeclareQueues());
        }

        #endregion Constructor(s)

        public Publisher<string> CreateStringPublisher(
            string exchangeName,
            string routingKey = ""
        )
        {
            return new SimplePublisher<string>(this.UsingConnection.CreateModel())
            {
                ExchangeName = exchangeName,
                RoutingKey = routingKey,
                InputSerializer = new StringSerializer()
            };
        }

        public Publisher<int, int> CreateInt32Publisher(
            string exchangeName,
            string routingKey = ""
        )
        {
            return new SimplePublisher<int, int>(this.UsingConnection)
            {
                ExchangeName = exchangeName,
                RoutingKey = routingKey,
                InputSerializer = new Int32Serializer(),
                OutputSerializer = new Int32Serializer()
            };
        }

        public Subscriber CreateStringSubscriber(
            string queueName,
            IProcessor<string> processor
        )
        {
            return new SimpleSubscriber<string>(
                this.UsingConnection.CreateModel(),
                queueName,
                processor
            )
            {
                InputSerializer = new StringSerializer()
            };
        }

        public Subscriber CreateInt32Subscriber(
            string queueName,
            IProcessor<int, int> processor
        )
        {
            return new SimpleSubscriber<int, int>(
                this.UsingConnection.CreateModel(),
                queueName,
                processor
            )
            {
                InputSerializer = new Int32Serializer(),
                OutputSerializer = new Int32Serializer()
            };
        }

        public EventWaitHandle GetProcessedNotifier(ISubscriber subscriber)
        {
            var notifier = new AutoResetEvent(false);
            subscriber.Processed += (sender, e) => notifier.Set();
            return notifier;
        }

        public Queue DeclareDynamicQueue(Exchange exchange)
        {
            var channel = this.UsingConnection.CreateModel();
            var queue = new SimpleQueue()
            {
                Name = Guid.NewGuid().ToString(),
                BindingKey = Guid.NewGuid().ToString(),
                IsDurable = false
            }.Declare(channel);

            if (exchange != null)
                return queue.BindExchange(channel, exchange.Name);

            return queue;
        }

        public void Dispose()
        {
            using (var channel = this.UsingConnection.CreateModel())
            {
                foreach (var exchange in this.DeclaredExchanges)
                    channel.ExchangeDelete(exchange.Name, false);

                foreach (var queue in this.DeclaredQueues)
                    channel.QueueDelete(queue.Name, false, false);

                channel.Close();
            }
            this.UsingConnection.Close();
            this.UsingConnection.Dispose();
        }
    }
}