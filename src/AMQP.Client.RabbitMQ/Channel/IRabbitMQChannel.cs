using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    public interface IRabbitMQChannel
    {
        public short ChannelId { get; }
        public bool IsOpen { get; }

        ValueTask HandleAsync(FrameHeader header);
        ValueTask<bool> TryOpenChannelAsync();
        ValueTask<bool> TryCloseChannelAsync();
    }
}
