using CodingCat.Serializers.Interfaces;
using RabbitMQ.Client;

namespace CodingCat.RabbitMq.Abstractions.Tests.Impls
{
    public class SimplePublisher<TInput> : Publisher<TInput>
    {
        public ISerializer<TInput> InputSerializer { get; set; }

        #region Constructor(s)

        public SimplePublisher(IConnection connection) : base(connection)
        {
        }

        #endregion Constructor(s)

        protected override byte[] ToBytes(TInput input)
        {
            return this.InputSerializer.ToBytes(input);
        }
    }

    public class SimplePublisher<TInput, TOutput>
        : Publisher<TInput, TOutput>
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