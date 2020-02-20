using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public abstract class ConsumerBase
    {
        public readonly string ConsumerTag;
        public readonly ushort ChannelId;
        protected readonly RabbitMQProtocol _protocol;
        public event Action Closed;
        public bool IsClosed { get; protected set; }
        internal ConsumerBase(string tag, ushort channel, RabbitMQProtocol protocol)
        {
            ConsumerTag = tag;
            ChannelId = channel;
            _protocol = protocol;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask Delivery(DeliverInfo info)
        {
            var result = await _protocol.Reader.ReadAsync(new ContentHeaderFullReader(ChannelId));
            _protocol.Reader.Advance();
            await ReadBodyMessage(new RabbitMQDeliver(info,ChannelId,_protocol), result.Message);
        }
        internal abstract ValueTask ReadBodyMessage(RabbitMQDeliver deliver, ContentHeader header);

    }
}
