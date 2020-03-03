using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class BaseExchange : IExchange
    {
        public string Name { get; set; }
        public string RawExchangeType => this.ExchangeType.ToString().ToLower();
        public ExchangeTypes ExchangeType { get; set; }

        public bool IsDurable { get; set; }
        public bool IsAutoDelete { get; set; }
        public IDictionary<string, object> Arguments { get; set; }

        public virtual IExchange Declare(IModel channel)
        {
            channel.ExchangeDeclare(
                exchange: this.Name,
                type: this.RawExchangeType,
                durable: this.IsDurable,
                autoDelete: this.IsAutoDelete,
                arguments: this.Arguments
            );
            return this;
        }
    }
}