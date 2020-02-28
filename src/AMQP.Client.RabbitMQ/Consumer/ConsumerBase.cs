using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public abstract class ConsumerBase
    {
        public readonly string ConsumerTag;
        public readonly ushort ChannelId;
        protected readonly RabbitMQProtocol _protocol;
        protected readonly SemaphoreSlim _writerSemaphore;
        public event Action Closed;
        public bool IsClosed { get; protected set; }
        internal ConsumerBase(string tag, ushort channel, RabbitMQProtocol protocol, SemaphoreSlim semaphore)
        {
            ConsumerTag = tag;
            ChannelId = channel;
            _protocol = protocol;
            _writerSemaphore = semaphore;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask Delivery(DeliverInfo info)
        {
            var contentResult = await _protocol.Reader.ReadAsync(new ContentHeaderFullReader(ChannelId)).ConfigureAwait(false);
            _protocol.Reader.Advance();
            await ProcessBodyMessage(new RabbitMQDeliver(info,ChannelId,_protocol,_writerSemaphore), contentResult.Message.BodySize).ConfigureAwait(false);
        }
        internal abstract ValueTask ProcessBodyMessage(RabbitMQDeliver deliver, long contentBodySize);

    }
}
