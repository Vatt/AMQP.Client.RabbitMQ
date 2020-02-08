using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public interface IRabbitMQFrameHandler
    {
        ValueTask HandleFrameAsync(FrameHeader header);
    }
}
