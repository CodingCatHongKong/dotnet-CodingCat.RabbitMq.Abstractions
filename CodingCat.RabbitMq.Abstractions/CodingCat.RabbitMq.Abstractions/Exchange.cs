using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class Exchange
    {
        public string Name { get; set; }
        public ExchangeTypes ExchangeType { get; set; }

        public bool IsDurable { get; set; }
        public bool IsAutoDelete { get; set; }
        public IDictionary<string, object> Arguments { get; set; }

        public virtual Exchange Declare(IModel channel)
        {
            channel.ExchangeDeclare(
                exchange: this.Name,
                type: this.ExchangeType.ToString().ToLower(),
                durable: this.IsDurable,
                autoDelete: this.IsAutoDelete,
                arguments: this.Arguments
            );
            return this;
        }
    }
}