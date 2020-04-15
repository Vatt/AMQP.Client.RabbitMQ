using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System.Buffers;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public interface IConsumable
    {
        ref ConsumeConf Conf { get;  }
        ValueTask OnDeliveryAsync(ref Deliver deliver);
        ValueTask OnContentAsync(ContentHeader header);
        ValueTask OnBodyAsync(ReadOnlySequence<byte> body);
    }
}
