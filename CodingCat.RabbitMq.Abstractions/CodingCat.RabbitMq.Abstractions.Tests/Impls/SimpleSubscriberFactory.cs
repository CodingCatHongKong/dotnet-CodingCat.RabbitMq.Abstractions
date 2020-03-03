using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Interfaces;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using ISubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class SimpleSubscriberFactory<TInput> : ISubscriberFactory
    {
        private string queueName { get; }

        private IProcessor<TInput> processor { get; }
        private ISerializer<TInput> inputSerializer { get; }

        #region Constructor(s)

        public SimpleSubscriberFactory(
            string queueName,
            IProcessor<TInput> processor,
            ISerializer<TInput> inputSerializer
        )
        {
            this.queueName = queueName;

            this.processor = processor;
            this.inputSerializer = inputSerializer;
        }

        #endregion Constructor(s)

        public ISubscriber GetSubscribed(IModel channel)
        {
            return new SimpleSubscriber<TInput>(
                channel,
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
        private string queueName { get; }

        private IProcessor<TInput, TOutput> processor { get; }
        private ISerializer<TInput> inputSerializer { get; }
        private ISerializer<TOutput> outputSerializer { get; }

        #region Constructor(s)

        public SimpleSubscriberFactory(
            string queueName,
            IProcessor<TInput, TOutput> processor,
            ISerializer<TInput> inputSerializer,
            ISerializer<TOutput> outputSerializer
        )
        {
            this.queueName = queueName;

            this.processor = processor;
            this.inputSerializer = inputSerializer;
            this.outputSerializer = outputSerializer;
        }

        #endregion Constructor(s)

        public ISubscriber GetSubscribed(IModel channel)
        {
            return new SimpleSubscriber<TInput, TOutput>(
                channel,
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