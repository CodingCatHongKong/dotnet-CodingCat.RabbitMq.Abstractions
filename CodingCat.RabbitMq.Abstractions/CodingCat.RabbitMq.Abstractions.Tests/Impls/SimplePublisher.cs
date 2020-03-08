using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class SimplePublisher<TInput> : BasePublisher<TInput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }
        public Exception LastException { get; private set; }

        #region Constructor(s)

        public SimplePublisher(IModel channel) : base(channel)
        {
        }

        #endregion Constructor(s)

        protected override byte[] ToBytes(TInput input)
        {
            return this.InputSerializer.ToBytes(input);
        }

        protected override void OnInOutError(Exception ex)
        {
            this.LastException = ex;
            base.OnInOutError(ex);
        }
    }

    public class SimplePublisher<TInput, TOutput>
        : BasePublisher<TInput, TOutput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }
        public ISerializer<TOutput> OutputSerializer { get; set; }

        #region Constructor(s)

        public SimplePublisher(IConnection connection) : base(connection)
        {
        }

        #endregion Constructor(s)

        protected override byte[] ToBytes(TInput input)
        {
            return this.InputSerializer.ToBytes(input);
        }

        protected override TOutput FromBytes(byte[] bytes)
        {
            return this.OutputSerializer.FromBytes(bytes);
        }
    }
}