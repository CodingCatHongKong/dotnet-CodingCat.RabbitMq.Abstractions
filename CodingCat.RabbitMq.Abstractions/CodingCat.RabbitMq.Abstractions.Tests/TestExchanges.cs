using CodingCat.Mq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Tests.Abstracts;
using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using CodingCat.Serializers.Impls;
using CodingCat.Serializers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CodingCat.RabbitMq.Abstractions.Tests
{
    [TestClass]
    public class TestExchanges : BaseTest
    {
        public ISerializer<string> StringSerializer { get; } = new StringSerializer();
        public ISerializer<int> Int32Serializer { get; } = new Int32Serializer();

        public SimpleProcessor<string> StringProcessor { get; private set; }
        public SimpleProcessor<int, int> IntProcessor { get; private set; }

        [TestInitialize]
        public void Init()
        {
            var directExchange = this.GetDeclaredExchange(ExchangeTypes.Direct);
            using (var channel = this.UsingConnection.CreateModel())
            {
                this.GetDeclaredQueue(nameof(Test_Direct_Deliveried))
                    .BindExchange(channel, directExchange.Name);
            }

            this.StringProcessor = new SimpleProcessor<string>();
            this.IntProcessor = new SimpleProcessor<int, int>(
                input => input += 1
            );
        }

        [TestMethod]
        public void Test_Direct_Deliveried()
        {
            const string QUEUE_NAME = nameof(Test_Direct_Deliveried);

            // Arrange
            var expected = Guid.NewGuid().ToString();

            var publisher = new SimplePublisher<string>(
                this.UsingConnection.CreateModel()
            )
            {
                ExchangeName = this.GetDeclaredExchange(ExchangeTypes.Direct).Name,
                RoutingKey = QUEUE_NAME,
                InputSerializer = this.StringSerializer
            };
            var subscriber = new SimpleSubscriber<string>(
                this.UsingConnection.CreateModel(),
                QUEUE_NAME,
                this.StringProcessor
            )
            {
                InputSerializer = this.StringSerializer
            };

            var notifier = this.GetProcessedNotifier(subscriber.Subscribe());

            // Act
            publisher.Send(expected);
            notifier.WaitOne();

            // Assert
            Assert.IsTrue(this.StringProcessor
                .ProcessedInputs
                .Contains(expected)
            );

            publisher.Dispose();
            subscriber.Dispose();
        }

        public EventWaitHandle GetProcessedNotifier(ISubscriber subscriber)
        {
            var notifier = new AutoResetEvent(false);
            subscriber.Processed += (sender, e) => notifier.Set();
            return notifier;
        }

        public Exchange GetDeclaredExchange(ExchangeTypes exchangeType)
        {
            return this.DeclaredExchanges.FirstOrDefault(exchange =>
                exchange.ExchangeType.Equals(exchangeType)
            );
        }

        public Queue GetDeclaredQueue(string queueName)
        {
            return this.DeclaredQueues.FirstOrDefault(queue =>
                queue.Name.Equals(queueName)
            );
        }

        protected override IEnumerable<Exchange> DeclareExchanges()
        {
            using (var channel = this.UsingConnection.CreateModel())
            {
                return new[]
                {
                    ExchangeTypes.Direct,
                    ExchangeTypes.Fanout
                }
                    .Select(type => new SimpleExchange()
                    {
                        Name = $"{nameof(TestExchanges)}.{type}",
                        ExchangeType = type
                    }.Declare(channel))
                    .ToArray();
            }
        }

        protected override IEnumerable<Queue> DeclareQueues()
        {
            using (var channel = this.UsingConnection.CreateModel())
            {
                return new Queue[]
                {
                    new SimpleQueue()
                    {
                        Name = nameof(Test_Direct_Deliveried),
                        BindingKey = nameof(Test_Direct_Deliveried)
                    }.Declare(channel)
                };
            }
        }
    }
}