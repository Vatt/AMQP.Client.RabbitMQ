using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public interface IConsumable
    {
        ValueTask OnDeliveryAsync(Deliver deliver, RabbitMQProtocolReader protocol);
    }
}
