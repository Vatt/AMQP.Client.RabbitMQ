using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public struct ExchangeDeclare
    {
        public readonly string Name;
        public readonly string Type;
        public readonly ushort ChannelId;
        public bool Passive;
        public bool Durable;
        public bool AutoDelete;
        public bool Internal;
        public bool NoWait;
        public Dictionary<string, object> Arguments;
        internal ExchangeDeclare(
            ushort channelId, string name, string type, bool passive = false, bool durable = false,
            bool autoDelete = false, bool _internal = false, bool nowait = false, Dictionary<string, object> arguments = null)
        {
            ChannelId = channelId;
            Name = name;
            Type = type;
            Passive = passive;
            Durable = durable;
            AutoDelete = autoDelete;
            NoWait = nowait;
            Internal = _internal;
            Arguments = arguments;
        }
        public static ExchangeDeclare Create(ushort channelId, string name, string type)
        {
            return new ExchangeDeclare(channelId, name, type);
        }
        public static ExchangeDeclare CreateNoWait(ushort channelId, string name, string type)
        {
            return new ExchangeDeclare(channelId, name, type, nowait: true);
        }
        public static ExchangeDeclare Create(ushort channelId, string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return new ExchangeDeclare(channelId, name, type, durable: durable, autoDelete: autoDelete, arguments: arguments);
        }
        public static ExchangeDeclare CreateNoWait(ushort channelId, string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            return new ExchangeDeclare(channelId, name, type, durable: durable, autoDelete: autoDelete, nowait: true, arguments: arguments);
        }
        public static ExchangeDeclare CreatePassive(ushort channelId, string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            return new ExchangeDeclare(channelId, name, type, durable: durable, autoDelete: autoDelete, passive: true, arguments: arguments);
        }
    }
}
