using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class Publisher : IPublisher
    {
        protected IModel Channel { get; }

        public string ExchangeName { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = false;

        #region Constructor(s)

        public Publisher(IModel channel)
        {
            this.Channel = channel;
        }

        #endregion Constructor(s)

        protected virtual IBasicProperties GetBasicProperties()
        {
            return this.Channel.CreateBasicProperties();
        }

        protected virtual void Send(
            byte[] body,
            IBasicProperties basicProperties
        )
        {
            this.Channel.BasicPublish(
                exchange: this.ExchangeName,
                routingKey: this.RoutingKey,
                mandatory: this.IsMandatory,
                basicProperties: basicProperties ?? this.GetBasicProperties(),
                body: body
            );
        }

        public void Dispose()
        {
            this.Channel?.Close();
            this.Channel?.Dispose();
        }
    }

    public abstract class Publisher<TInput>
        : Publisher, IPublisher<TInput>
    {
        #region Constructor(s)

        public Publisher(IModel channel) : base(channel)
        {
        }

        #endregion Constructor(s)

        protected abstract byte[] ToBytes(TInput input);

        public virtual void Send(TInput input)
        {
            this.Send(this.ToBytes(input), null);
        }
    }

    public abstract class Publisher<TInput, TOutput>
        : Publisher, IPublisher<TInput, TOutput>
    {
        protected IConnection Connection { get; }

        #region Constructor(s)

        public Publisher(IConnection connection)
            : base(connection.CreateModel())
        {
            this.Connection = connection;
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