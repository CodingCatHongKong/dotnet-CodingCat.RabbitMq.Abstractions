using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingCat.RabbitMq.Abstractions.Tests.Abstracts
{
    public abstract class BaseTest : IDisposable
    {
        public IConnection UsingConnection { get; }
        public List<Exchange> DeclaredExchanges { get; } = new List<Exchange>();
        public List<Queue> DeclaredQueues { get; } = new List<Queue>();

        protected abstract IEnumerable<Exchange> DeclareExchanges();
        protected abstract IEnumerable<Queue> DeclareQueues();

        #region Constructor(s)
        public BaseTest()
        {
            this.UsingConnection = new ConnectionFactory()
            {
                Uri = new Uri(Constants.USING_RABBITMQ)
            }.CreateConnection();

            this.DeclaredExchanges.AddRange(this.DeclareExchanges());
            this.DeclaredQueues.AddRange(this.DeclareQueues());
        }
        #endregion

        public void Dispose()
        {
            using(var channel = this.UsingConnection.CreateModel())
            {
                foreach (var exchange in this.DeclaredExchanges)
                    channel.ExchangeDelete(exchange.Name, false);

                foreach (var queue in this.DeclaredQueues)
                    channel.QueueDelete(queue.Name, false, false);

                channel.Close();
            }
            this.UsingConnection.Close();
            this.UsingConnection.Dispose();
        }
    }
}
