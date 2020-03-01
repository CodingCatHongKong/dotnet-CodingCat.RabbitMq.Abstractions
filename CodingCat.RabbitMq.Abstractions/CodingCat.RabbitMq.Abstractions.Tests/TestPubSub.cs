using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using CodingCat.Serializers.Impls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CodingCat.RabbitMq.Abstractions.Tests
{
    [TestClass]
    public class TestPubSub : IDisposable
    {
        public IConnection UsingConnection { get; }
        public List<Queue> Queues { get; } = new List<Queue>();

        public SimpleProcessor<string> StringInputProcessor { get; private set; }

        #region Constructor(s)

        public TestPubSub()
        {
            this.UsingConnection = new ConnectionFactory()
            {
                Uri = new Uri(Constants.USING_RABBITMQ)
            }.CreateConnection();

            using (var channel = this.UsingConnection.CreateModel())
            {
                foreach (var queueName in new string[]
                {
                    nameof(Test_StringInputSubscriber_AreProcessed),
                    nameof(Test_ResponseSubscriber_IsExpected)
                })
                {
                    this.Queues.Add(new SimpleQueue()
                    {
                        Name = queueName
                    }.Declare(channel));
                }
                channel.Close();
            }
        }

        #endregion Constructor(s)

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
            var notifier = new AutoResetEvent(false);

            var channel = this.UsingConnection.CreateModel();
            var serializer = new StringSerializer();

            var publisher = new SimplePublisher<string>(channel)
            {
                InputSerializer = serializer,
                RoutingKey = QUEUE_NAME
            };
            var subscriber = new SimpleSubscriber<string>(
                channel,
                QUEUE_NAME,
                this.StringInputProcessor
            )
            {
                InputSerializer = serializer
            }.Subscribe();

            // Act
            subscriber.Processed += (sender, e) => notifier.Set();
            publisher.Send(expected);
            notifier.WaitOne();

            // Assert
            Assert.IsTrue(this.StringInputProcessor
                .ProcessedInputs
                .Contains(expected)
            );
            subscriber.Dispose();
        }

        [TestMethod]
        public void Test_ResponseSubscriber_IsExpected()
        {
            const string QUEUE_NAME = nameof(Test_ResponseSubscriber_IsExpected);

            // Arrange
            var input = new Random().Next(0, 1000);
            var expected = input + 1;

            var channel = this.UsingConnection.CreateModel();
            var serializer = new Int32Serializer();

            var publisher = new SimplePublisher<int, int>(
                this.UsingConnection
            )
            {
                InputSerializer = serializer,
                OutputSerializer = serializer,
                RoutingKey = QUEUE_NAME
            };
            var processor = new SimpleProcessor<int, int>(
                val => val + 1
            );
            var subscriber = new SimpleSubscriber<int, int>(
                channel,
                QUEUE_NAME,
                processor
            )
            {
                InputSerializer = serializer,
                OutputSerializer = serializer
            }.Subscribe();

            // Act
            var actual = publisher.Send(input);

            // Assert
            Assert.AreEqual(expected, actual);
            subscriber.Dispose();
        }

        public void Dispose()
        {
            using(var channel = this.UsingConnection.CreateModel())
            {
                foreach (var queue in this.Queues)
                    channel.QueueDelete(queue.Name, false, false);
                channel.Close();
            }
            this.UsingConnection.Close();
            this.UsingConnection.Dispose();
        }
    }
}