using CodingCat.Mq.Abstractions.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using IBaseSubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;
using ISubscriber = CodingCat.RabbitMq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class Subscriber : ISubscriber
    {
        protected IModel Channel { get; }

        public string QueueName { get; }
        public string ConsumerTag { get; } = Guid.NewGuid().ToString();
        public bool IsAutoAck { get; set; } = true;

        public event EventHandler Subscribed;

        public abstract event EventHandler Processed;

        public event EventHandler Disposing;

        public event EventHandler Disposed;

        #region Constructor(s)

        public Subscriber(IModel channel, string queueName)
        {
            this.Channel = channel;
            this.QueueName = queueName;
        }

        #endregion Constructor(s)

        public virtual IBaseSubscriber Subscribe()
        {
            var consumer = new EventingBasicConsumer(this.Channel);
            consumer.Received += this.OnReceived;

            this.Channel.BasicConsume(
                queue: this.QueueName,
                autoAck: this.IsAutoAck,
                consumerTag: this.ConsumerTag,
                consumer: consumer
            );
            this.Subscribed?.Invoke(this, null);
            return this;
        }

        protected abstract void OnReceived(
            object sender,
            BasicDeliverEventArgs args
        );

        public virtual void Dispose()
        {
            this.Disposing?.Invoke(this, null);

            this.Channel?.BasicCancel(this.ConsumerTag);
            this.Channel?.Close();
            this.Channel?.Dispose();

            this.Disposed?.Invoke(this, null);
        }
    }

    public abstract class Subscriber<TInput> : Subscriber
    {
        protected IProcessor<TInput> Processor { get; }

        public override event EventHandler Processed;

        #region Constructor(s)

        public Subscriber(
            IModel channel,
            string queueName,
            IProcessor<TInput> processor
        ) : base(channel, queueName)
        {
            this.Processor = processor;
        }

        #endregion Constructor(s)

        protected abstract TInput FromBytes(byte[] bytes);

        protected override void OnReceived(
            object sender,
            BasicDeliverEventArgs args
        )
        {
            var input = this.FromBytes(args.Body);
            this.Processor.HandleInput(input);

            this.Processed?.Invoke(this, null);
        }
    }

    public abstract class Subscriber<TInput, TOutput> : Subscriber
    {
        protected IProcessor<TInput, TOutput> Processor { get; }

        public override event EventHandler Processed;

        #region Constructor(s)

        public Subscriber(
            IModel channel,
            string queueName,
            IProcessor<TInput, TOutput> processor
        ) : base(channel, queueName)
        {
            this.Processor = processor;
        }

        #endregion Constructor(s)

        protected abstract TInput FromBytes(byte[] bytes);

        protected abstract byte[] ToBytes(TOutput output);

        protected override void OnReceived(
            object sender,
            BasicDeliverEventArgs args
        )
        {
            var input = this.FromBytes(args.Body);
            var output = this.Processor.ProcessInput(input);
            var replyTo = args.BasicProperties.ReplyTo;

            if (!string.IsNullOrEmpty(replyTo))
            {
                this.Channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: replyTo,
                    body: this.ToBytes(output)
                );
            }

            this.Processed?.Invoke(this, null);
        }
    }
}