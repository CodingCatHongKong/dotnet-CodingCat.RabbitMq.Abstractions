using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class SimpleSubscriber<TInput> : BaseSubscriber<TInput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }
        public Exception LastException { get; private set; }

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

        protected override void OnSerializationError(Exception ex)
        {
            this.LastException = ex;
            base.OnSerializationError(ex);
        }
    }

    public class SimpleSubscriber<TInput, TOutput>
        : BaseSubscriber<TInput, TOutput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }
        public ISerializer<TOutput> OutputSerializer { get; set; }
        public Exception LastException { get; private set; }

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

        protected override void OnSerializationError(Exception ex)
        {
            this.LastException = ex;
            base.OnSerializationError(ex);
        }
    }
}