using System.Buffers;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;

namespace AMQP.Client.RabbitMQ.Consumer
{
    internal interface IConsumable
    {
        ref ConsumeConf Conf { get; }
        ValueTask OnDeliveryAsync(ref Deliver deliver);
        ValueTask OnContentAsync(ContentHeader header);
        ValueTask OnBodyAsync(ReadOnlySequence<byte> body);
    }
}