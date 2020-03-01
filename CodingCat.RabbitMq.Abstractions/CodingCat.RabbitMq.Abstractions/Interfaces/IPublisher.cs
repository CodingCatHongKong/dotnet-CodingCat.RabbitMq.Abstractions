using System;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface IPublisher : IDisposable
    {
        string ExchangeName { get; }
        string RoutingKey { get; }
        bool IsMandatory { get; }
    }

    public interface IPublisher<TInput> : IPublisher
    {
        void Send(TInput input);
    }

    public interface IPublisher<TInput, TOutput> : IPublisher
    {
        TOutput Send(TInput input);
    }
}