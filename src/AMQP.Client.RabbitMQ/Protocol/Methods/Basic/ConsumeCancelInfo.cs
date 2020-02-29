using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct ConsumeCancelInfo
    {
        public readonly string ConsumerTag;
        public readonly bool NoWait;
        public ConsumeCancelInfo(string consumerTag, bool noWait)
        {
            ConsumerTag = consumerTag;
            NoWait = noWait;
        }
    }
}
