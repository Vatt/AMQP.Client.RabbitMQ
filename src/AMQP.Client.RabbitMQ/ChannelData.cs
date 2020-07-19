using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System.Collections.Generic;
using System.Threading;

namespace AMQP.Client.RabbitMQ
{
    internal class ChannelData
    {        
        public Dictionary<string, QueueBind> Binds = new Dictionary<string, QueueBind>();
        public Dictionary<string, IConsumable> Consumers = new Dictionary<string, IConsumable>();
        public Dictionary<string, ExchangeDeclare> Exchanges = new Dictionary<string, ExchangeDeclare>();
        public Dictionary<string, QueueDeclare> Queues = new Dictionary<string, QueueDeclare>();
        public SemaphoreSlim WriterSemaphore = new SemaphoreSlim(1);
        public bool IsClosed = false;        
    }
}