using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class SimpleSubscriber<TInput> : Subscriber<TInput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }

        #region Constructor(s)

        public SimpleSubscriber(
            IModel channel,
            string queueName,
            IProcessor<TInput> processor
        ) : base(channel, queueName, processor) { }

        #endregion Constructor(s)

        protected override TInput FromBytes(byte[] bytes)
        {
            return this.InputSerializer.FromBytes(bytes);
        }
    }

    public class SimpleSubscriber<TInput, TOutput>
        : Subscriber<TInput, TOutput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }
        public ISerializer<TOutput> OutputSerializer { get; set; }

        #region Constructor(s)

        public SimpleSubscriber(
            IModel channel,
            string queueName,
            IProcessor<TInput, TOutput> processor
        ) : base(channel, queueName, processor) { }

        #endregion Constructor(s)

        protected override TInput FromBytes(byte[] bytes)
        {
            return this.InputSerializer.FromBytes(bytes);
        }

        protected override byte[] ToBytes(TOutput output)
        {
            return this.OutputSerializer.ToBytes(output);
        }
    }
}