using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct  QueueInfo
    {
        public readonly string Name;
        public readonly bool Passive;
        public readonly bool Durable;
        public readonly bool Exclusive;
        public readonly bool AutoDelete;
        public readonly bool NoWait;
        public readonly Dictionary<string, object> Arguments;
        public QueueInfo(string name, bool durable = false, bool exclusive = false,
                         bool autoDelete = false, bool passive = false, bool nowait = false,
                         Dictionary<string, object> arguments = null)
        {
            Name = name;
            Passive = passive;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            NoWait = nowait;
            Arguments = arguments;
        }
    }
}
