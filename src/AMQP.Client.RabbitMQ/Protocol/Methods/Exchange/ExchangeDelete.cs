namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public readonly struct ExchangeDelete
    {
        public readonly ushort ChannelId;
        public readonly string Name;
        public readonly bool IfUnused;
        public readonly bool NoWait;
        internal ExchangeDelete(ushort channelId, string name, bool ifUnused, bool nowait = false)
        {
            ChannelId = channelId;
            Name = name;
            IfUnused = ifUnused;
            NoWait = nowait;
        }
        public static ExchangeDelete Create(ushort channelId, string name, bool ifUnused = false) => new ExchangeDelete(channelId, name, ifUnused);
        public static ExchangeDelete CreateNoWait(ushort channelId, string name, bool ifUnused = false) => new ExchangeDelete(channelId, name, ifUnused, true);
    }
}
