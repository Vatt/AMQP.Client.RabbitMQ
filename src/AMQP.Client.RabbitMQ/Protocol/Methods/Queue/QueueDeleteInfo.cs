using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct QueueDeleteInfo
    {
        public readonly string QueueName;
        public readonly bool IfUnused;
        public readonly bool IfEmpty;
        public readonly bool NoWait;
        public QueueDeleteInfo(string queueName, bool ifUnused, bool ifEmpty, bool noWait)
        {
            QueueName = queueName;
            IfUnused = ifUnused;
            IfEmpty = ifEmpty;
            NoWait = noWait;
        }
    }
}
