using System;
using System.Collections.Generic;
using System.Threading;
using CodingCat.RabbitMq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Tests.Abstracts;
using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using CodingCat.Serializers.Impls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodingCat.RabbitMq.Abstractions.Tests
{
    [TestClass]
    public class TestSubscriberIssuesHandling : BaseTest
    {
        [TestMethod]
        public void Test_TInputSerializerSerializerException_IsHandled()
        {
            // Arrange
            var queueName = nameof(Test_TInputSerializerSerializerException_IsHandled);
            var publisher = this.CreateStringPublisher(
                string.Empty,
                queueName
            );

            var processor = new SimpleProcessor<string>();
            var subscriber = new SimpleSubscriberFactory<string>(
                queueName,
                processor,
                new NotImplementedSerializer<string>()
            ).GetSubscribed(this.UsingConnection.CreateModel());

            var notifier = new AutoResetEvent(false);

            // Act
            publisher.Send(string.Empty);
            notifier.WaitOne(100);

            // Assert
            Assert.IsNotNull((subscriber as SimpleSubscriber<string>).LastException);
        }

        [TestMethod]
        public void Test_TOutputSubscriberSerializerException_IsHandled()
        {
            // Arrange
            var queueName = nameof(Test_TOutputSubscriberSerializerException_IsHandled);
            var publisher = this.CreateInt32Publisher(
                string.Empty,
                queueName
            );

            var subscriber = new SimpleSubscriberFactory<int, int>(
                queueName,
                new SimpleProcessor<int, int>(val => val),
                new NotImplementedSerializer<int>(),
                new NotImplementedSerializer<int>()
            ).GetSubscribed(this.UsingConnection.CreateModel());

            var notifier = this.GetProcessedNotifier(subscriber);

            // Act
            publisher.Send(1);
            notifier.WaitOne();

            // Assert
            Assert.IsNotNull((subscriber as SimpleSubscriber<int, int>).LastException);
        }

        [TestMethod]
        public void Test_TOutputSubscriberTimeout_IsReturnedDefault()
        {
            // Arrange
            var queueName = nameof(Test_TOutputSubscriberTimeout_IsReturnedDefault);
            var publisher = this.CreateInt32Publisher(
                string.Empty,
                queueName
            );

            var expected = new Random().Next(10, 100);
            var subscriber = new SimpleSubscriberFactory<int, int>(
                queueName,
                new SimpleProcessor<int, int>(val =>
                {
                    Thread.Sleep(1500);
                    return val;
                })
                {
                    Timeout = TimeSpan.FromMilliseconds(10),
                    DefaultOutput = expected
                },
                new Int32Serializer(),
                new Int32Serializer()
            ).GetSubscribed(this.UsingConnection.CreateModel());

            // Act
            var actual = publisher.Send(1);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        protected override IEnumerable<IExchange> DeclareExchanges()
        {
            return new IExchange[] { };
        }

        protected override IEnumerable<IQueue> DeclareQueues()
        {
            using(var channel = this.UsingConnection.CreateModel())
            {
                return new IQueue[]
                {
                    new SimpleQueue()
                    {
                        Name = nameof(Test_TInputSerializerSerializerException_IsHandled),
                        IsDurable = false
                    }.Declare(channel),
                    new SimpleQueue()
                    {
                        Name = nameof(Test_TOutputSubscriberSerializerException_IsHandled),
                        IsDurable = false
                    }.Declare(channel),
                    new SimpleQueue()
                    {
                        Name = nameof(Test_TOutputSubscriberTimeout_IsReturnedDefault),
                        IsDurable = false
                    }.Declare(channel)
                };
            }
        }
    }
}
