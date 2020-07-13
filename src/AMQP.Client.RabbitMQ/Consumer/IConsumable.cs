using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    internal interface IConsumable
    {
        ref ConsumeConf Conf { get; }
        ValueTask OnBeginDeliveryAsync(RabbitMQDeliver deliver, RabbitMQProtocolReader protocol);
    }
}