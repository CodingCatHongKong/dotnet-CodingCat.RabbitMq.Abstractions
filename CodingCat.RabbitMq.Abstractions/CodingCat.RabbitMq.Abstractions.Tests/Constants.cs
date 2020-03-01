using CodingCat.RabbitMq.Abstractions.Tests.Impls;

namespace CodingCat.RabbitMq.Abstractions.Tests
{
    public static class Constants
    {
        public const string USING_RABBITMQ = "amqp://localhost/test";

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