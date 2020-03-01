using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;

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

    public abstract class Publisher<TInput, TOutput> : Publisher
    {
        #region Constructor(s)

        public Publisher(IConnection connection) : base(connection)
        {
        }

        #endregion Constructor(s)

        protected abstract byte[] ToBytes(TInput input);

        protected abstract TOutput FromBytes(byte[] bytes);

        public virtual TOutput Send(TInput input)
        {
            using (var channel = this.Connection.CreateModel())
            {
                var replyTo = channel
                    .QueueDeclare(exclusive: false)
                    .QueueName;
                var properties = this.GetBasicProperties();

                properties.ReplyTo = replyTo;

                this.Send(this.ToBytes(input), properties);

                var output = this.GetFromReplyQueue(channel, replyTo);
                channel.Close();

                return output;
            }
        }

        protected virtual TOutput GetFromReplyQueue(
            IModel channel,
            string replyTo
        )
        {
            var output = default(TOutput);
            var notifier = new AutoResetEvent(false);

            var consumerTag = Guid.NewGuid().ToString();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, e) =>
            {
                output = this.FromBytes(e.Body);
                notifier.Set();
            };
            channel.BasicConsume(
                queue: replyTo,
                autoAck: true,
                consumerTag: consumerTag,
                consumer: consumer
            );

            notifier.WaitOne();
            channel.BasicCancel(consumerTag);

            return output;
        }
    }
}