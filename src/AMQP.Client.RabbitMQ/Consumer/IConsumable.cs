using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    internal interface IConsumable
    {
        ref ConsumeConf Conf { get; }
        ValueTask OnBeginDeliveryAsync(Deliver deliver, RabbitMQProtocolReader protocol);
    }
}