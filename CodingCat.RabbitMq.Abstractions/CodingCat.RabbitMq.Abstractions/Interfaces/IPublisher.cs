namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface IPublisher
    {
        string ExchangeName { get; }
        string RoutingKey { get; }
        bool IsMandatory { get; }
    }

    public interface IPublisher<TInput>
    {
        void Send(TInput input);
    }

    public interface IPublisher<TInput, TOutput>
    {
        TOutput Send(TInput input);
    }
}