using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Text;

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
        internal abstract void Delivery(DeliverInfo info);
    }
}
