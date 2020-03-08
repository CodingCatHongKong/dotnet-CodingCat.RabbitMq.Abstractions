using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class BaseQueue : IQueue
    {
        public string Name { get; set; } = string.Empty;
        public string BindingKey { get; set; } = string.Empty;

        public bool IsDurable { get; set; } = true;
        public bool IsExclusive { get; set; } = false;
        public bool IsAutoDelete { get; set; } = false;
        public IDictionary<string, object> Arguments { get; set; }

        public virtual IQueue Declare(IModel channel)
        {
            channel.QueueDeclare(
                queue: this.Name,
                durable: this.IsDurable,
                exclusive: this.IsExclusive,
                autoDelete: this.IsAutoDelete,
                arguments: this.Arguments
            );
            return this;
        }

        public virtual IQueue BindExchange(
            IModel channel,
            string exchangeName,
            IDictionary<string, object> arguments = null
        )
        {
            channel.QueueBind(
                queue: this.Name,
                exchange: exchangeName,
                routingKey: this.BindingKey,
                arguments: arguments
            );
            return this;
        }
    }
}