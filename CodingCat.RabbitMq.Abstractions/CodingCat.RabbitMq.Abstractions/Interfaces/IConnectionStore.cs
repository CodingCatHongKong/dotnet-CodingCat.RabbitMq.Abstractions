using RabbitMQ.Client;
using System;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface IConnectionStore
    {
        IConnectionStore Add(Enum key, IConnection connection);

        IConnectionStore Add(string key, IConnection connection);

        IConnection Get(Enum key);

        IConnection Get(string key);
    }
}