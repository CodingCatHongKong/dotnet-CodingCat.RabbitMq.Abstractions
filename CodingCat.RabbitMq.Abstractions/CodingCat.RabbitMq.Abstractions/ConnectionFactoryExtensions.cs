using CodingCat.RabbitMq.Abstractions.Interfaces;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.Abstractions
{
    public static class ConnectionFactoryExtensions
    {
        public static IConnection CreateConnection(
            this IConnectionFactory factory,
            IConnectConfiguration configuration
        )
        {
            return factory.CreateConnection(
                configuration.TimeoutPerTry,
                configuration.RetryInterval,
                configuration.RetryUpTo
            );
        }

        public static IConnection CreateConnection(
            this IConnectionFactory factory,
            TimeSpan timeoutPerTry,
            TimeSpan retryInterval,
            uint retryUpTo
        )
        {
            for (var i = 0; i < retryUpTo; i++)
            {
                var connection = factory.CreateConnection(timeoutPerTry);
                if (connection != null) return connection;
                if (i < retryUpTo) Thread.Sleep(retryInterval);
            }

            return null;
        }

        public static IConnection CreateConnection(
            this IConnectionFactory factory,
            TimeSpan timeout
        )
        {
            var connection = null as IConnection;
            var connectedOrTimedOutEvent = new AutoResetEvent(false);

            Task.Delay(timeout)
                .ContinueWith(task => connectedOrTimedOutEvent.Set());
            Task.Run(() =>
            {
                connection = factory.CreateConnection();
                if (!connection.IsOpen) connection = null;
                connectedOrTimedOutEvent.Set();
            });

            connectedOrTimedOutEvent.WaitOne();
            return connection;
        }
    }
}