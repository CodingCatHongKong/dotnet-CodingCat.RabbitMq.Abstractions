using CodingCat.RabbitMq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Tests.Abstracts;
using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using CodingCat.Serializers.Impls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodingCat.RabbitMq.Abstractions.Tests
{
    [TestClass]
    public class TestPubSub : BaseTest
    {
        public SimpleProcessor<string> StringInputProcessor { get; private set; }

        [TestInitialize]
        public void Init()
        {
            this.StringInputProcessor = new SimpleProcessor<string>();
        }

        [TestMethod]
        public void Test_StringInputSubscriber_AreProcessed()
        {
            const string QUEUE_NAME = nameof(Test_StringInputSubscriber_AreProcessed);

            // Arrange
            var expected = Guid.NewGuid().ToString();
            var serializer = new StringSerializer();

            var publisher = this.CreateStringPublisher(
                string.Empty,
                QUEUE_NAME
            );
            var subscriber = this.CreateStringSubscriber(
                QUEUE_NAME,
                this.StringInputProcessor
            );

            // Act
            using (subscriber)
            {
                var notifier = this.GetProcessedNotifier(subscriber);

                using (publisher) publisher.Send(expected);
                notifier.WaitOne();
            }

            // Assert
            Assert.IsTrue(this.StringInputProcessor
                .ProcessedInputs
                .Contains(expected)
            );
        }

        [TestMethod]
        public void Test_ResponseSubscriber_IsExpected()
        {
            const string QUEUE_NAME = nameof(Test_ResponseSubscriber_IsExpected);

            // Arrange
            var input = new Random().Next(0, 1000);
            var expected = input + 1;

            var publisher = this.CreateInt32Publisher(
                string.Empty,
                QUEUE_NAME
            );
            var subscriber = this.CreateInt32Subscriber(
                QUEUE_NAME,
                new SimpleProcessor<int, int>(val => val + 1)
            );

            // Act
            var actual = int.MinValue;
            using (subscriber)
            using (publisher)
                actual = publisher.Send(input);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        protected override IEnumerable<IExchange> DeclareExchanges()
        {
            return new BaseExchange[] { };
        }

        protected override IEnumerable<IQueue> DeclareQueues()
        {
            using (var channel = this.UsingConnection.CreateModel())
            {
                return new string[]
                {
                    nameof(Test_StringInputSubscriber_AreProcessed),
                    nameof(Test_ResponseSubscriber_IsExpected)
                }
                    .Select(name => new SimpleQueue()
                    {
                        Name = name
                    }.Declare(channel))
                    .ToArray();
            }
        }
    }
}