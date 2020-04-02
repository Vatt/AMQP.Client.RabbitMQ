using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    public readonly struct RabbitMQChannel
    {
        private readonly ChannelHandler _handler;
        public readonly ushort ChannelId;
        internal RabbitMQChannel(ushort id, ChannelHandler handler)
        {
            ChannelId = id;
            _handler = handler;
        }
        public ValueTask ExchangeDeclareAsync(Exchange exchange)
        {
            return _handler.ExchangeDeclareAsync(this, exchange);
        }
        public ValueTask ExchangeDeleteAsync(ExchangeDelete exchange)
        {
            return _handler.ExchangeDeleteAsync(this, exchange);
        }
        public ValueTask<QueueDeclareOk> QueueDeclareAsync(Queue queue)
        {
            return _handler.QueueDeclareAsync(this, queue);
        }
        public ValueTask QueueDeclareNoWaitAsync(Queue queue)
        {
            queue.NoWait = true;
            return _handler.QueueDeclareNoWaitAsync(this, queue);
        }
        public ValueTask<int> QueueDeleteAsync(QueueDelete queue)
        {
            return _handler.QueueDeleteAsync(this, queue);
        }
        public ValueTask QueueDeleteNoWaitAsync(QueueDelete queue)
        {
            queue.NoWait = true;
            return _handler.QueueDeleteNoWaitAsync(this, queue);
        }
    }
}
