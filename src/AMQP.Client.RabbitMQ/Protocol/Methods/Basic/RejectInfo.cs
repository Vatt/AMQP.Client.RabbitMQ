namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct RejectInfo
    {
        public readonly long DeliveryTag;
        public readonly ushort ChannelId;
        public readonly bool Requeue;
        internal RejectInfo(ushort channelId, long tag, bool requeue)
        {
            ChannelId = channelId;
            DeliveryTag = tag;
            Requeue = requeue;
        }
        public static RejectInfo Create(ushort channelId, long deliveryTag, bool requeue) => new RejectInfo(channelId, deliveryTag, requeue);
    }
}
