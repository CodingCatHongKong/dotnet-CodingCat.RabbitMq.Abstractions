using CodingCat.RabbitMq.Abstractions.Interfaces;
using CodingCat.RabbitMq.Abstractions.Tests.Abstracts;
using CodingCat.RabbitMq.Abstractions.Tests.Impls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.Abstractions.Tests
{
    [TestClass]
    public class TestPublisherIssueHandling : BaseTest
    {
        [TestMethod]
        public void Test_NoResponse_IsTimedOut()
        {
            const int TIMEOUT_SECONDS = 3;
            const string QUEUE = nameof(Test_NoResponse_IsTimedOut);

            // Arrange
            var publisher = this.CreateInt32Publisher(
                string.Empty,
                QUEUE,
                TIMEOUT_SECONDS / 2
            );
            var subscriber = this.CreateInt32Subscriber(
                QUEUE,
                new SimpleProcessor<int, int>(val =>
                {
                    Thread.Sleep(TIMEOUT_SECONDS * 2000);
                    throw new Exception();
                })
            ).Subscribe();

            var notifier = new AutoResetEvent(false);
            var isManualTimedOut = false;
            var timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS);

            // Act
            Task.Delay(timeout)
                .ContinueWith(t =>
                {
                    isManualTimedOut = true;
                    notifier.Set();
                });
            Task.Run(() =>
            {
                publisher.Send(0);
                notifier.Set();
            });

            notifier.WaitOne();

            // Assert
            Assert.IsFalse(isManualTimedOut);
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
                    nameof(Test_NoResponse_IsTimedOut)
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