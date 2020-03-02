using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using CodingCat.Serializers.Impls;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using ISubscriber = CodingCat.RabbitMq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions.Tests.Abstracts
{
    public abstract class BaseTest : IDisposable
    {
        public IConnection UsingConnection { get; }
        public List<IExchange> DeclaredExchanges { get; } = new List<IExchange>();
        public List<IQueue> DeclaredQueues { get; } = new List<IQueue>();

        protected abstract IEnumerable<IExchange> DeclareExchanges();
        protected abstract IEnumerable<IQueue> DeclareQueues();

        #region Constructor(s)

        public BaseTest()
        {
            this.UsingConnection = new ConnectionFactory()
            {
                Uri = new Uri(Constants.USING_RABBITMQ)
            }.CreateConnection(Constants.ConnectConfiguration);

            this.DeclaredExchanges.AddRange(this.DeclareExchanges());
            this.DeclaredQueues.AddRange(this.DeclareQueues());
        }

        #endregion Constructor(s)

        public IPublisher<string> CreateStringPublisher(
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

        public IPublisher<int, int> CreateInt32Publisher(
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

        public ISubscriber CreateStringSubscriber(
            string queueName,
            IProcessor<string> processor
        )
        {
            return new SimpleSubscriberFactory<string>(
                this.UsingConnection,
                queueName,
                processor,
                new StringSerializer()
            ).GetSubscriber();
        }

        public ISubscriber CreateInt32Subscriber(
            string queueName,
            IProcessor<int, int> processor
        )
        {
            return new SimpleSubscriberFactory<int, int>(
                this.UsingConnection,
                queueName,
                processor,
                new Int32Serializer(),
                new Int32Serializer()
            ).GetSubscriber();
        }

        public EventWaitHandle GetProcessedNotifier(ISubscriber subscriber)
        {
            var notifier = new AutoResetEvent(false);
            subscriber
                .Subscribe()
                .Processed += (sender, e) => notifier.Set();
            return notifier;
        }

        public IQueue DeclareDynamicQueue(IExchange exchange)
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