namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct ConsumeCancelInfo
    {
        public readonly string ConsumerTag;
        public readonly ushort ChannelId;
        public readonly bool NoWait;
        public ConsumeCancelInfo(ushort channelId, string consumerTag, bool noWait)
        {
            ChannelId = channelId;
            ConsumerTag = consumerTag;
            NoWait = noWait;
        }
    }
}
