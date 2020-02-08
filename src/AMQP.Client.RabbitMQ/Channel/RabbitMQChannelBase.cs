using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    public abstract class RabbitMQChannelBase
    {
        public readonly short Id;
        public abstract bool IsOpen { get; }
        public RabbitMQChannelBase(short id)
        {
            Id = id;
        }
        public abstract ValueTask HandleFrameAsync(FrameHeader header);
        public abstract ValueTask<bool> TryOpenChannelAsync();
        public abstract ValueTask<bool> TryCloseChannelAsync();
    }
}
