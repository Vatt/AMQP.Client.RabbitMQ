using System;

namespace AMQP.Client.RabbitMQ.Exceptions
{
    public class ChannelClosedException: InvalidOperationException
    {
        public ChannelClosedException(ushort channelId) : base($"Channel {channelId} already closed")
        {
            
        }
    }
}