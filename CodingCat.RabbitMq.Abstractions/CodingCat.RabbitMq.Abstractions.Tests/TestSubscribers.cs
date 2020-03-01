using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using CodingCat.Serializers.Impls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;
using System.Threading;

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

        public void Dispose()
        {
            this.UsingConnection.Close();
            this.UsingConnection.Dispose();
        }
    }
}