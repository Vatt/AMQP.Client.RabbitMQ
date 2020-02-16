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
        public bool IsClosed { get; protected set; }
        internal ConsumerBase(string tag)
        {
            ConsumerTag = tag;
        }
        internal abstract ValueTask Delivery(DeliverInfo info, ContentHeader header);
    }
}
