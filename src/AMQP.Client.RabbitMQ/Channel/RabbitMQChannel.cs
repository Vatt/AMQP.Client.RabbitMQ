using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    public class RabbitMQChannel : RabbitMQChannelReader,IRabbitMQChannel
    {
        public short ChannelId => throw new NotImplementedException();

        public bool IsOpen => throw new NotImplementedException();

        public ValueTask HandleAsync(FrameHeader header)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> TryCloseChannelAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> TryOpenChannelAsync()
        {
            throw new NotImplementedException();
        }
    }
}
