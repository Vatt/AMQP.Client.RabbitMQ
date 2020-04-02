using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public struct Exchange
    {
        public readonly string Name;
        public readonly string Type;
        public bool Passive;
        public bool Durable;
        public bool AutoDelete;
        public bool Internal;
        public bool NoWait;
        public Dictionary<string, object> Arguments;
        internal Exchange(
            string name, string type, bool passive = false, bool durable = false,
            bool autoDelete = false, bool _internal = false, bool nowait = false,
            Dictionary<string, object> arguments = null)
        {
            Name = name;
            Type = type;
            Passive = passive;
            Durable = durable;
            AutoDelete = autoDelete;
            NoWait = nowait;
            Internal = _internal;
            Arguments = arguments;
        }
        public static Exchange Create(string name, string type)
        {
            return new Exchange(name, type);
        }
        public static Exchange CreateNoWait(string name, string type)
        {
            return new Exchange(name, type, nowait: true);
        }
        public static Exchange Create(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return new Exchange(name, type, durable: durable, autoDelete: autoDelete, arguments: arguments);
        }
        public static Exchange CreateNoWait(string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            return new Exchange(name, type, durable: durable, autoDelete: autoDelete, nowait: true, arguments: arguments);
        }
        public static Exchange CreatePassive(string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            return new Exchange(name, type, durable: durable, autoDelete: autoDelete, passive: true, arguments: arguments);
        }
    }
}
