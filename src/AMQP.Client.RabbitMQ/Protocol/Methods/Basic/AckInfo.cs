namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct AckInfo
    {
        public readonly ushort ChannelId;
        public readonly long DeliveryTag;
        public readonly bool Multiple;
        internal AckInfo(ushort channelId, long deliveryTag, bool multiple)
        {
            ChannelId = channelId;
            DeliveryTag = deliveryTag;
            Multiple = multiple;
        }
        public static AckInfo Create(ushort channelId, long deliveryTag, bool multiple = false) => new AckInfo(channelId, deliveryTag, multiple);
    }
}
