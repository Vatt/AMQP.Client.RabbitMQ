namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct QoSInfo
    {
        public readonly int PrefetchSize;
        public readonly ushort PrefetchCount;
        public readonly ushort ChannelId;
        public readonly bool Global;
        internal QoSInfo(ushort channelId, int prefetchSize, ushort prefetchCount, bool global)
        {
            ChannelId = channelId;
            PrefetchSize = prefetchSize;
            PrefetchCount = prefetchCount;
            Global = global;
        }
        public static QoSInfo Create(ushort channelId, int prefetchSize, ushort prefetchCount, bool global = false) => new QoSInfo(channelId, prefetchSize, prefetchCount, global);
    }
}
