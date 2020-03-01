using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using CodingCat.Serializers.Impls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.Abstractions.Tests
{
    [TestClass]
    public class TestSubscribers : IDisposable
    {
        public IConnection UsingConnection { get; }
        public SimpleProcessor<string> StringInputProcessor { get; private set; }

        #region Constructor(s)

        public TestSubscribers()
        {
            this.UsingConnection = new ConnectionFactory()
            {
                Uri = new Uri(Constants.USING_RABBITMQ)
            }.CreateConnection();
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

            using (var publishChannel = this.UsingConnection.CreateModel())
                publishChannel.BasicPublish(
                    "",
                    QUEUE_NAME,
                    false,
                    null,
                    serializer.ToBytes(expected)
                );

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

            var notifier = new ManualResetEvent(false);

            var channel = this.UsingConnection.CreateModel();
            var serializer = new Int32Serializer();

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
            var actual = int.MinValue;

            var replyQueue = channel.QueueDeclare(
                "", false, false, true, null
            ).QueueName;
            Task.Run(() =>
            {
                var usingChannel = this.UsingConnection.CreateModel();
                var consumerTag = Guid.NewGuid().ToString();
                var consumer = new EventingBasicConsumer(usingChannel);
                consumer.Received += (sender, e) =>
                {
                    actual = serializer.FromBytes(e.Body);
                    notifier.Set();
                };

                usingChannel.BasicConsume(
                    replyQueue, true, consumerTag, consumer
                );
                notifier.WaitOne();
                usingChannel.BasicCancel(consumerTag);
                usingChannel.Close();
                usingChannel.Dispose();
            });

            var properties = channel.CreateBasicProperties();
            properties.ReplyTo = replyQueue;
            using (var publishChannel = this.UsingConnection.CreateModel())
                publishChannel.BasicPublish(
                    "",
                    QUEUE_NAME,
                    false,
                    properties,
                    serializer.ToBytes(input)
                );

            notifier.WaitOne();

            // Assert
            Assert.AreEqual(expected, actual);
            subscriber.Dispose();
        }

        public void Dispose()
        {
            this.UsingConnection.Close();
            this.UsingConnection.Dispose();
        }
    }
}