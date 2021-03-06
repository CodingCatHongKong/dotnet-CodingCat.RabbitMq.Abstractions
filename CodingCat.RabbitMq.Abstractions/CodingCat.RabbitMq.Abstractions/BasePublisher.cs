﻿using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class BasePublisher : IPublisher
    {
        protected IModel Channel { get; }

        public string ExchangeName { get; set; } = string.Empty;
        public string RoutingKey { get; set; } = string.Empty;
        public bool IsMandatory { get; set; } = false;

        #region Constructor(s)

        public BasePublisher(IModel channel)
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

        protected virtual void OnInOutError(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        protected byte[] GetBody(Func<byte[]> callback)
        {
            try { return callback(); }
            catch (Exception ex) { this.OnInOutError(ex); }

            return new byte[0];
        }

        public void Dispose()
        {
            this.Channel?.Close();
            this.Channel?.Dispose();
        }
    }

    public abstract class BasePublisher<TInput>
        : BasePublisher, IPublisher<TInput>
    {
        #region Constructor(s)

        public BasePublisher(IModel channel) : base(channel)
        {
        }

        #endregion Constructor(s)

        protected abstract byte[] ToBytes(TInput input);

        public virtual void Send(TInput input)
        {
            this.Send(this.GetBody(() => this.ToBytes(input)), null);
        }
    }

    public abstract class BasePublisher<TInput, TOutput>
        : BasePublisher, IPublisher<TInput, TOutput>
    {
        protected IConnection Connection { get; }

        public TOutput DefaultOutput { get; set; } = default(TOutput);
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(0);

        public bool IsTimeoutEnabled => this.Timeout.TotalMilliseconds > 0;

        #region Constructor(s)

        public BasePublisher(IConnection connection)
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
                var body = this.GetBody(() => this.ToBytes(input));

                properties.ReplyTo = replyTo;

                this.Send(body, properties);

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
            var notifier = new AutoResetEvent(false);
            var output = this.DefaultOutput;

            var consumerTag = Guid.NewGuid().ToString();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (sender, e) =>
            {
                try
                {
                    output = this.FromBytes(e.Body);
                }
                catch (Exception ex)
                {
                    this.OnInOutError(ex);
                }
                notifier.Set();
            };
            channel.BasicConsume(
                queue: replyTo,
                autoAck: true,
                consumerTag: consumerTag,
                consumer: consumer
            );

            this.WaitFor(notifier);
            channel.BasicCancel(consumerTag);

            return output;
        }

        protected void WaitFor(EventWaitHandle notifier)
        {
            if (this.IsTimeoutEnabled)
                using (notifier)
                    notifier.WaitOne(this.Timeout);
            else
                using (notifier)
                    notifier.WaitOne();
        }
    }
}