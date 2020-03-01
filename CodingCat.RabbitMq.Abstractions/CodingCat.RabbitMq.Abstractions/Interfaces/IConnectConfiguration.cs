using System;

namespace CodingCat.RabbitMq.Abstractions.Interfaces
{
    public interface IConnectConfiguration
    {
        TimeSpan TimeoutPerTry { get; }
        TimeSpan RetryInterval { get; }
        uint RetryUpTo { get; }
    }
}