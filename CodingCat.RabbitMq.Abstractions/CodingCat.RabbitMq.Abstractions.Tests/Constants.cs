using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using System;

namespace CodingCat.RabbitMq.Abstractions.Tests
{
    public static class Constants
    {
        public const string USING_RABBITMQ = "amqp://localhost/test";

        public static IConnectConfiguration ConnectConfiguration = new ConnectConfiguration()
        {
            TimeoutPerTry = TimeSpan.FromSeconds(30),
            RetryInterval = TimeSpan.FromSeconds(3),
            RetryUpTo = 3
        };

        public static Exchange DirectExchange = new SimpleExchange()
        {
            Name = $"{nameof(DirectExchange)}.{ExchangeTypes.Direct}",
            ExchangeType = ExchangeTypes.Direct,
            IsDurable = false
        };

        public static Exchange FanoutExchange = new SimpleExchange()
        {
            Name = $"{nameof(FanoutExchange)}.{ExchangeTypes.Fanout}",
            ExchangeType = ExchangeTypes.Fanout,
            IsDurable = false
        };

        public static Queue DirectQueue = new SimpleQueue()
        {
            Name = nameof(DirectQueue),
            BindingKey = nameof(DirectQueue),
            IsDurable = false
        };
    }
}