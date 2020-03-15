﻿namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public readonly struct ExchangeDeleteInfo
    {
        public readonly string Name;
        public readonly bool IfUnused;
        public readonly bool NoWait;
        public ExchangeDeleteInfo(string name, bool ifUnused, bool nowait = false)
        {
            Name = name;
            IfUnused = ifUnused;
            NoWait = nowait;
        }
    }
}
