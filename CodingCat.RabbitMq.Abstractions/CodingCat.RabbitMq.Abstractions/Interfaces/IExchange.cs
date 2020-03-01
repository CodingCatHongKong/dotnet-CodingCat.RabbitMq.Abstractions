using RabbitMQ.Client;
using System.Collections.Generic;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface IExchange
    {
        string Name { get; }
        string RawExchangeType { get; }

        bool IsDurable { get; }
        bool IsAutoDelete { get; }
        IDictionary<string, object> Arguments { get; }

        IExchange Declare(IModel channel);
    }
}