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
        protected IInitializer RabbitMqInitializer { get; private set; }

        #region Constructor(s)

        public BaseHostFactory(
            IConnection connection,
            IInitializer rabbitMqInitializer
        )
        {
            this.Connection = connection;
            this.RabbitMqInitializer = rabbitMqInitializer;
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
            var subscribers = this.RabbitMqInitializer
                .Configure(this.Connection.CreateModel())
                .GetSubscribed(this.Connection);

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