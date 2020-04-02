namespace AMQP.Client.RabbitMQ
{
    public struct RabbitMQChannel
    {
        public readonly ushort ChannelId;
        public RabbitMQChannel(ushort id)
        {
            ChannelId = id;
        }
    }
}
