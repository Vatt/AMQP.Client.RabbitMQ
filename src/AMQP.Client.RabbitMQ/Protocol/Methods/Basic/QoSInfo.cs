namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct QoSInfo
    {
        public readonly int PrefetchSize;
        public readonly ushort PrefetchCount;
        public readonly bool Global;
        internal QoSInfo(int prefetchSize, ushort prefetchCount, bool global)
        {
            PrefetchSize = prefetchSize;
            PrefetchCount = prefetchCount;
            Global = global;
        }
        public static QoSInfo Create(int prefetchSize, ushort prefetchCount, bool global = false) => new QoSInfo(prefetchSize, prefetchCount, global);
    }
}
