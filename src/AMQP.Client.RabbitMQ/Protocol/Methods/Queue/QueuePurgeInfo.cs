using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct  QueuePurgeInfo
    {
        public readonly string QueueName;
        public readonly bool NoWait;
        public QueuePurgeInfo(string queueName, bool noWait)
        {
            QueueName = queueName;
            NoWait = noWait;
        }
    }
}
