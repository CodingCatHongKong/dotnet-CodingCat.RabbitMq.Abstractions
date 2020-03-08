using CodingCat.Mq.Abstractions.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using IBaseSubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;
using ISubscriber = CodingCat.RabbitMq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class BaseSubscriber : ISubscriber
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

        public BaseSubscriber(IModel channel, string queueName)
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

        protected virtual void OnSerializationError(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        public virtual void Dispose()
        {
            this.Disposing?.Invoke(this, null);

            this.Channel?.BasicCancel(this.ConsumerTag);
            this.Channel?.Close();
            this.Channel?.Dispose();

            this.Disposed?.Invoke(this, null);
        }
    }

    public abstract class BaseSubscriber<TInput> : BaseSubscriber
    {
        protected IProcessor<TInput> Processor { get; }

        public override event EventHandler Processed;

        #region Constructor(s)

        public BaseSubscriber(
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
            var input = default(TInput);

            try
            {
                input = this.FromBytes(args.Body);
            }
            catch (Exception ex)
            {
                this.OnSerializationError(ex);
                return;
            }

            this.Processor.HandleInput(input);
            this.Processed?.Invoke(this, null);
        }
    }

    public abstract class BaseSubscriber<TInput, TOutput> : BaseSubscriber
    {
        protected IProcessor<TInput, TOutput> Processor { get; }

        public override event EventHandler Processed;

        #region Constructor(s)

        public BaseSubscriber(
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
            var input = this.Processor.DefaultInput;
            var output = this.Processor.DefaultOutput;

            try
            {
                input = this.FromBytes(args.Body);
            }
            catch (Exception ex)
            {
                this.OnSerializationError(ex);
            }

            output = this.Processor.ProcessInput(input);

            var replyTo = args.BasicProperties.ReplyTo;
            if (!string.IsNullOrEmpty(replyTo))
            {
                var body = new byte[0];

                try
                {
                    body = this.ToBytes(output);
                }
                catch (Exception ex)
                {
                    this.OnSerializationError(ex);
                }

                this.Channel.BasicPublish(
                    exchange: string.Empty,
                    routingKey: replyTo,
                    body: body
                );
            }

            this.Processed?.Invoke(this, null);
        }
    }
}