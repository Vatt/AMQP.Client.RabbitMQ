using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    public class ChannelData
    {
        internal Dictionary<string, QueueBind> Binds = new Dictionary<string, QueueBind>();
        internal Dictionary<string, IConsumable> Consumers = new Dictionary<string, IConsumable>();
        internal Dictionary<string, ExchangeDeclare> Exchanges = new Dictionary<string, ExchangeDeclare>();
        internal Dictionary<string, QueueDeclare> Queues = new Dictionary<string, QueueDeclare>();

        public TaskCompletionSource<string> ConsumeTcs;
        public TaskCompletionSource<QueueDeclareOk> QueueTcs;
        public TaskCompletionSource<int> CommonTcs;

        internal SemaphoreSlim WriterSemaphore = new SemaphoreSlim(1);
        internal RabbitMQSession Session;
        internal TaskCompletionSource<bool> waitTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        internal bool IsClosed = false;
        internal ChannelData(RabbitMQSession session)
        {
            Session = session;
            waitTcs.SetResult(false);
        }
    }
}