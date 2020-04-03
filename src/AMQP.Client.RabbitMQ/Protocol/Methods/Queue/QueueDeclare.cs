using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public struct QueueDeclare
    {
        public readonly string Name;
        public readonly bool Passive;
        public readonly bool Durable;
        public readonly bool Exclusive;
        public readonly bool AutoDelete;
        public bool NoWait;
        public readonly Dictionary<string, object> Arguments;
        internal QueueDeclare
            (string name, bool durable = false, bool exclusive = false,
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
        public static QueueDeclare Create(string name)
        {
            return new QueueDeclare(name);
        }
        public static QueueDeclare CreatePassive(string name)
        {
            return new QueueDeclare(name, passive: true);
        }
        public static QueueDeclare Create(string name, bool durable = false, bool exclusive = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return new QueueDeclare(name, durable, exclusive, autoDelete, arguments: arguments);
        }

    }
}
