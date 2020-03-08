using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface IQueue
    {
        string Name { get; }
        string BindingKey { get; }

        bool IsDurable { get; }
        bool IsExclusive { get; }
        bool IsAutoDelete { get; }
        IDictionary<string, object> Arguments { get; }

        IQueue Declare(IModel channel);

        IQueue BindExchange(
            IModel channel,
            string exchangeName,
            IDictionary<string, object> arguments = null
        );
    }
}