using CodingCat.RabbitMq.Abstractions.Interfaces;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ISubscriber = CodingCat.Mq.Abstractions.Interfaces.ISubscriber;

namespace CodingCat.RabbitMq.Abstractions
{
    public abstract class BaseHostFactory : BackgroundService
    {
        protected IConnection Connection { get; private set; }
        protected ISubscriberFactory[] SubscriberFactories { get; private set; }

        #region Constructor(s)

        public BaseHostFactory(
            IConnection connection,
            IEnumerable<ISubscriberFactory> subscriberFactories
        )
        {
            this.Connection = connection;
            this.SubscriberFactories = subscriberFactories.ToArray();
        }

        #endregion Constructor(s)

        protected override Task ExecuteAsync(
            CancellationToken stoppingToken
        )
        {
            return Task.Run(() => this.Subscribe(stoppingToken));
        }

        protected virtual void Subscribe(CancellationToken stoppingToken)
        {
            var subscribers = this.SubscriberFactories
                .Select(factory =>
                {
                    var channel = this.Connection.CreateModel();
                    return factory.GetSubscribed(channel);
                })
                .ToArray();

            Console.WriteLine(new StringBuilder()
                .Append($"Subscribed {subscribers.Count()} ")
                .Append("subscriber(s)")
                .ToString()
            );

            stoppingToken.Register(() => Dispose(subscribers));
        }

        public override void Dispose()
        {
            using (this.Connection)
                this.Connection?.Dispose();

            base.Dispose();
        }

        public static void Dispose(IEnumerable<ISubscriber> subscribers)
        {
            foreach (var subscriber in subscribers)
                subscriber?.Dispose();
        }
    }
}