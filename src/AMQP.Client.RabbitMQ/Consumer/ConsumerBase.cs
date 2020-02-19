using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public abstract class ConsumerBase
    {
        public readonly string ConsumerTag;
        public readonly ushort ChannelId;
        public bool IsClosed { get; protected set; }
        internal ConsumerBase(string tag, ushort channel)
        {
            ConsumerTag = tag;
            ChannelId = channel;
        }
        internal abstract ValueTask Delivery(DeliverInfo info);
    }
}
