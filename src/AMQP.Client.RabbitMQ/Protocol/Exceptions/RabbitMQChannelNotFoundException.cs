namespace AMQP.Client.RabbitMQ.Protocol.Exceptions
{
    public class RabbitMQChannelNotFoundException : RabbitMQException
    {
        public RabbitMQChannelNotFoundException(ushort channelId) : base($"ChannelId: {channelId} not found")
        {

        }
    }
}
