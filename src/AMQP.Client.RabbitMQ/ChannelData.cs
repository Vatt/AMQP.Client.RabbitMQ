using System;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;

namespace AMQP.Client.RabbitMQ
{
    public class ChannelData : IDisposable
    {
        internal Dictionary<string, QueueBind> Binds = new Dictionary<string, QueueBind>();
        internal Dictionary<string, IConsumable> Consumers = new Dictionary<string, IConsumable>();
        internal Dictionary<string, ExchangeDeclare> Exchanges = new Dictionary<string, ExchangeDeclare>();
        internal Dictionary<string, QueueDeclare> Queues = new Dictionary<string, QueueDeclare>();

        internal TaskCompletionSource<string> ConsumeTcs;
        internal TaskCompletionSource<QueueDeclareOk> QueueTcs;
        internal TaskCompletionSource<int> CommonTcs;

        internal SemaphoreSlim WriterSemaphore = new SemaphoreSlim(1);
        internal RabbitMQSession? Session;
        internal bool IsClosed = false;

        public event EventHandler<RabbitMQCloseArgs> ChanelClosed;
        internal ChannelData(RabbitMQSession session)
        {
            Session = session;
        }

        public void Dispose()
        {
            Session = null;
            IsClosed = true;
            Binds.Clear();
            Binds = null;
            Exchanges.Clear();
            Exchanges = null;
            Queues.Clear();
            Queues = null;
            if (ConsumeTcs != null && !ConsumeTcs.Task.IsCompleted)
            {
                ConsumeTcs.SetCanceled();
            }
            if (QueueTcs != null && !QueueTcs.Task.IsCompleted)
            {
                ConsumeTcs.SetCanceled();
            }
            if (CommonTcs != null && !CommonTcs.Task.IsCompleted)
            {
                ConsumeTcs.SetCanceled();
            }
            WriterSemaphore.Dispose();
            try
            {
                foreach (var consumerPair in Consumers)
                {
                    consumerPair.Value.OnConsumerCancelAsync();
                }
            }finally
            {
                Consumers.Clear();
                Consumers = null;
            }
    
            
            
        }

        internal virtual void onChanelClosed(RabbitMQCloseArgs e)
        {
            ChanelClosed?.Invoke(this, e);
        }
    }
}