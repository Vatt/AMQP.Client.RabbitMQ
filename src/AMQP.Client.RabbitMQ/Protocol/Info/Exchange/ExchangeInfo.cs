using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Info.Exchange
{
    public struct ExchangeInfo
    {
        public readonly short ChannelId;
        public readonly string Name;
        public readonly string Type;
        public bool Passive;
        public bool Durable;
        public bool AutoDelete;
        public bool Internal;
        public bool NoWait;
        public Dictionary<string, object> Arguments;
        public ExchangeInfo(short channelId, string name, string type, bool passive, bool durable,
                                   bool autoDelete, bool _internal,bool nowait, Dictionary<string, object> arguments)
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
    }
}
