using System;
using IBaseSubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface ISubscriber : IBaseSubscriber
    {
        string QueueName { get; }
        string ConsumerTag { get; }
        bool IsAutoAck { get; }
    }
}