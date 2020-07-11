using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System.Buffers;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public interface IChannelHandler
    {
        ValueTask OnChannelOpenOkAsync(ushort channelId);
        ValueTask OnChannelCloseAsync(ushort channelId, CloseInfo info);
        ValueTask OnChannelCloseOkAsync(ushort channelId);

        ValueTask OnQueueDeclareOkAsync(ushort channelId, QueueDeclareOk declare);
        ValueTask OnQueueBindOkAsync(ushort channelId);
        ValueTask OnQueueUnbindOkAsync(ushort channelId);
        ValueTask OnQueuePurgeOkAsync(ushort channelId, int purged);
        ValueTask OnQueueDeleteOkAsync(ushort channelId, int deleted);


        ValueTask OnExchangeDeclareOkAsync(ushort channelId);
        ValueTask OnExchangeDeleteOkAsync(ushort channelId);

        ValueTask OnBeginDeliveryAsync(ushort channelId, Deliver deliver, RabbitMQProtocolReader protocol);
        ValueTask OnConsumeOkAsync(ushort channelId, string tag);
        ValueTask OnQosOkAsync(ushort channelId);
        ValueTask OnConsumerCancelOkAsync(ushort channelId, string tag);
    }
}