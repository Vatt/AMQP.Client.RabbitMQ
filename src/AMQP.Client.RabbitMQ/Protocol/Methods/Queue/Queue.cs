using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public struct Queue
    {
        public readonly string Name;
        public readonly bool Passive;
        public readonly bool Durable;
        public readonly bool Exclusive;
        public readonly bool AutoDelete;
        public bool NoWait;
        public readonly Dictionary<string, object> Arguments;
        internal Queue
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
        public static Queue Create(string name)
        {
            return new Queue(name);
        }
        public static Queue CreatePassive(string name)
        {
            return new Queue(name, passive: true);
        }
        public static Queue Create(string name, bool durable = false, bool exclusive = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return new Queue(name, durable, exclusive, autoDelete, arguments: arguments);
        }

    }
}
