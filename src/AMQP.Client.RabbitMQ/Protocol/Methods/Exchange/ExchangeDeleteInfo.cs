using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public readonly struct  ExchangeDeleteInfo
    {
        public readonly ushort ChannelId;
        public readonly string Name;
        public readonly bool IfUnused;
        public readonly bool NoWait;
        public ExchangeDeleteInfo(ushort channelId, string name, bool ifUnused, bool nowait = false)
        {
            ChannelId = channelId;
            Name = name;
            IfUnused = ifUnused;
            NoWait = nowait;
        }
    }
}
