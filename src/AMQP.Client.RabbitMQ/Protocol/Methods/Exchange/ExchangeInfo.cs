using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public struct ExchangeInfo
    {
        public readonly string Name;
        public readonly string Type;
        public bool Passive;
        public bool Durable;
        public bool AutoDelete;
        public bool Internal;
        public bool NoWait;
        public Dictionary<string, object> Arguments;
        public ExchangeInfo(string name, string type, bool passive = false, bool durable = false,
                            bool autoDelete = false, bool _internal = false, bool nowait = false, Dictionary<string, object> arguments = null)
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
    }
}
