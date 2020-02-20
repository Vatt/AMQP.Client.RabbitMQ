using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct RejectInfo
    {
        public readonly long DeliveryTag;
        public readonly bool Requeue;
        public RejectInfo(long tag, bool requeue)
        {
            DeliveryTag = tag;
            Requeue = requeue;
        }
    }
}
