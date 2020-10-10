using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;

namespace AMQP.Client.RabbitMQ.Consumer
{
    internal interface IConsumable
    {
        ref ConsumeConf Conf { get; }
        ValueTask OnBeginDeliveryAsync(RabbitMQDeliver deliver, ProtocolReader protocol);
        ValueTask OnConsumerCancelAsync();
    }
}