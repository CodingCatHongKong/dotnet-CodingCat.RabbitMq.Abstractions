using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Interfaces;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using ISubscriber = CodingCat.RabbitMq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class SimpleSubscriberFactory<TInput> : ISubscriberFactory
    {
        private IConnection connection { get; }
        private string queueName { get; }

        private IProcessor<TInput> processor { get; }
        private ISerializer<TInput> inputSerializer { get; }

        #region Constructor(s)

        public SimpleSubscriberFactory(
            IConnection connection,
            string queueName,
            IProcessor<TInput> processor,
            ISerializer<TInput> inputSerializer
        )
        {
            this.connection = connection;
            this.queueName = queueName;

            this.processor = processor;
            this.inputSerializer = inputSerializer;
        }

        #endregion Constructor(s)

        public ISubscriber GetSubscriber()
        {
            return new SimpleSubscriber<TInput>(
                this.connection.CreateModel(),
                this.queueName,
                this.processor
            )
            {
                InputSerializer = this.inputSerializer,
                IsAutoAck = true
            };
        }
    }

    public class SimpleSubscriberFactory<TInput, TOutput>
        : ISubscriberFactory
    {
        private IConnection connection { get; }
        private string queueName { get; }

        private IProcessor<TInput, TOutput> processor { get; }
        private ISerializer<TInput> inputSerializer { get; }
        private ISerializer<TOutput> outputSerializer { get; }

        #region Constructor(s)

        public SimpleSubscriberFactory(
            IConnection connection,
            string queueName,
            IProcessor<TInput, TOutput> processor,
            ISerializer<TInput> inputSerializer,
            ISerializer<TOutput> outputSerializer
        )
        {
            this.connection = connection;
            this.queueName = queueName;

            this.processor = processor;
            this.inputSerializer = inputSerializer;
            this.outputSerializer = outputSerializer;
        }

        #endregion Constructor(s)

        public ISubscriber GetSubscriber()
        {
            return new SimpleSubscriber<TInput, TOutput>(
                this.connection.CreateModel(),
                this.queueName,
                this.processor
            )
            {
                InputSerializer = this.inputSerializer,
                OutputSerializer = this.outputSerializer,
                IsAutoAck = true
            };
        }
    }
}