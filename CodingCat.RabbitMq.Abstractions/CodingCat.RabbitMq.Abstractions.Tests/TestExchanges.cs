using CodingCat.RabbitMq.Abstractions.Interfaces;
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
        public SimpleProcessor<string> StringProcessor { get; private set; }

        [TestInitialize]
        public void Init()
        {
            this.StringProcessor = new SimpleProcessor<string>();
        }

        [TestMethod]
        public void Test_Direct_Deliveried()
        {
            // Arrange
            var expected = Guid.NewGuid().ToString();

            var publisher = this.CreateStringPublisher(
                Constants.DirectExchange.Name,
                Constants.DirectQueue.Name
            );
            var subscriber = this.CreateStringSubscriber(
                Constants.DirectQueue.Name,
                this.StringProcessor
            );

            // Act
            using (subscriber)
            {
                var notifier = this.GetProcessedNotifier(subscriber);

                using (publisher) publisher.Send(expected);
                notifier.WaitOne();
            }

            // Assert
            Assert.IsTrue(this.StringProcessor
                .ProcessedInputs
                .Contains(expected)
            );
        }

        [TestMethod]
        public void Test_Fanout_Deliveried()
        {
            const int FANOUT_CLIENTS_AMOUNT = 3;

            // Arrange
            var expected = Guid.NewGuid().ToString();
            var exchange = Constants.FanoutExchange;
            var queues = new string[FANOUT_CLIENTS_AMOUNT]
                .Select(_ => this.DeclareDynamicQueue(exchange))
                .ToArray();

            var publisher = this.CreateStringPublisher(exchange.Name);
            var subscribers = queues
                .Select(queue =>
                    this.CreateStringSubscriber(
                        queue.Name,
                        this.StringProcessor
                    ).Subscribe()
                )
                .ToArray();

            this.DeclaredQueues.AddRange(queues);

            // Act
            using (publisher) publisher.Send(expected);

            while (this.StringProcessor.ProcessedInputs.Count < FANOUT_CLIENTS_AMOUNT)
                Thread.Sleep(10);

            // Assert
            Assert.IsTrue(this.StringProcessor
                .ProcessedInputs
                .Contains(expected)
            );
            Assert.AreEqual(
                FANOUT_CLIENTS_AMOUNT,
                this.StringProcessor.ProcessedInputs.Count()
            );

            foreach (var subscriber in subscribers)
                subscriber.Dispose();
        }

        protected override IEnumerable<IExchange> DeclareExchanges()
        {
            using (var channel = this.UsingConnection.CreateModel())
            {
                return new[]
                {
                    Constants.DirectExchange.Declare(channel),
                    Constants.FanoutExchange.Declare(channel)
                };
            }
        }

        protected override IEnumerable<IQueue> DeclareQueues()
        {
            using (var channel = this.UsingConnection.CreateModel())
            {
                return new[]
                {
                    Constants.DirectQueue.Declare(channel)
                        .BindExchange(channel, Constants.DirectExchange.Name)
                };
            }
        }
    }
}