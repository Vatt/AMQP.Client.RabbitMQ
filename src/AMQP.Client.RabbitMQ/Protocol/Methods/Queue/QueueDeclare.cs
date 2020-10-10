using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public struct QueueDeclare
    {
        public readonly ushort ChannelId;
        public readonly string Name;
        public readonly bool Passive;
        public readonly bool Durable;
        public readonly bool Exclusive;
        public readonly bool AutoDelete;
        public bool NoWait;
        public readonly Dictionary<string, object> Arguments;
        internal QueueDeclare
            (ushort channelId, string name, bool durable = false, bool exclusive = false,
            bool autoDelete = false, bool passive = false, bool nowait = false,
            Dictionary<string, object> arguments = null)
        {
            ChannelId = channelId;
            Name = name;
            Passive = passive;
            Durable = durable;
            Exclusive = exclusive;
            AutoDelete = autoDelete;
            NoWait = nowait;
            Arguments = arguments;
        }
        public static QueueDeclare Create(ushort channelId, string name)
        {
            return new QueueDeclare(channelId, name);
        }
        public static QueueDeclare CreatePassive(ushort channelId, string name)
        {
            return new QueueDeclare(channelId, name, passive: true);
        }
        public static QueueDeclare Create(ushort channelId, string name, bool durable = false, bool exclusive = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return new QueueDeclare(channelId, name, durable, exclusive, autoDelete, arguments: arguments);
        }

    }
}
