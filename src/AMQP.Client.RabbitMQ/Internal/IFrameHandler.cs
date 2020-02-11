using AMQP.Client.RabbitMQ.Protocol.Framing;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal interface IFrameHandler
    {
        ValueTask HandleAsync(FrameHeader header);
    }
}
