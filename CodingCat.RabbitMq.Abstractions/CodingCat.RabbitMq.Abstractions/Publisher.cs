using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class Publisher : IDisposable
    {
        private IModel usingChannel { get; }

        protected IConnection Connection { get; }

        public string ExchangeName { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = null;
        public bool IsMandatory { get; set; } = false;

        #region Constructor(s)

        public Publisher(IConnection connection)
        {
            this.Connection = connection;

            this.usingChannel = this.Connection.CreateModel();
        }

        #endregion Constructor(s)

        protected virtual IBasicProperties GetBasicProperties()
        {
            return this.usingChannel.CreateBasicProperties();
        }

        protected virtual void Send(
            byte[] body,
            IBasicProperties basicProperties
        )
        {
            this.usingChannel.BasicPublish(
                exchange: this.ExchangeName,
                routingKey: this.RoutingKey,
                mandatory: this.IsMandatory,
                basicProperties: basicProperties ?? this.GetBasicProperties(),
                body: body
            );
        }

        public void Dispose()
        {
            this.usingChannel?.Close();
            this.usingChannel?.Dispose();
        }
    }

    public abstract class Publisher<TInput> : Publisher
    {
        #region Constructor(s)

        public Publisher(IConnection connection) : base(connection)
        {
        }

        #endregion Constructor(s)

        protected abstract byte[] ToBytes(TInput input);

        public virtual void Send(TInput input)
        {
            this.Send(this.ToBytes(input), null);
        }
    }
}