using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ExchangeDeclareAsync(ExchangeDeclare exchange)
        {
            return _handler.ExchangeDeclareAsync(this, exchange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ExchangeDeleteAsync(ExchangeDelete exchange)
        {
            return _handler.ExchangeDeleteAsync(this, exchange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<QueueDeclareOk> QueueDeclareAsync(QueueDeclare queue)
        {
            return _handler.QueueDeclareAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueDeclareNoWaitAsync(QueueDeclare queue)
        {
            queue.NoWait = true;
            return _handler.QueueDeclareNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> QueueDeleteAsync(QueueDelete queue)
        {
            return _handler.QueueDeleteAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueDeleteNoWaitAsync(QueueDelete queue)
        {
            queue.NoWait = true;
            return _handler.QueueDeleteNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> QueuePurgeAsync(QueuePurge queue)
        {
            return _handler.QueuePurgeAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueuePurgeNoWaitAsync(QueuePurge queue)
        {
            queue.NoWait = true;
            return _handler.QueuePurgeNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueBindAsync(QueueBind bind)
        {
            return _handler.QueueBindAsync(this, bind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueUnbindAsync(QueueUnbind unbind)
        {
            return _handler.QueueUnbindAsync(this, unbind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ConsumerStartAsync(RabbitMQConsumer consumer)
        {
            return _handler.ConsumerStartAsync(consumer);
        }
    }
}
