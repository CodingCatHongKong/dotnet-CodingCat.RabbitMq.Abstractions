namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface ISubscriberFactory
    {
        ISubscriber GetSubscriber();
    }
}