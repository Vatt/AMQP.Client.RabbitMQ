using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public static class QueuePorotocolExtension
    {
        private static readonly QueueDeclareOkReader _queueDeclareOkReader = new QueueDeclareOkReader();
        private static readonly QueuePurgeOkDeleteOkReader _queuePurgeOkDeleteOkReader = new QueuePurgeOkDeleteOkReader();
        public static ValueTask SendQueueDeclare(this RabbitMQProtocol protocol, ushort channelId, QueueInfo info)
        {
            return protocol.WriteAsync(new QueueDeclareWriter(channelId), info);
        }
        public static ValueTask<QueueDeclareOk> ReadQueueDeclareOk(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_queueDeclareOkReader, token);
        }
        public static ValueTask SendQueueBind(this RabbitMQProtocol protocol, ushort channelId, QueueBindInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new QueueBindWriter(channelId), info, token);
        }

        public static ValueTask SendQueueUnbind(this RabbitMQProtocol protocol, ushort channelId, QueueUnbindInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new QueueUnbindWriter(channelId), info, token);
        }
        public static ValueTask SendQueuePurge(this RabbitMQProtocol protocol, ushort channelId, QueuePurgeInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new QueuePurgeWriter(channelId), info, token);
        }
        public static ValueTask SendQueueDelete(this RabbitMQProtocol protocol, ushort channelId, QueueDeleteInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new QueueDeleteWriter(channelId), info, token);
        }
        public static ValueTask<int> ReadQueuePurgeOkDeleteOk(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_queuePurgeOkDeleteOkReader, token);
        }
        public static ValueTask<bool> ReadBindOkUnbindOk(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadNoPayload(token);
        }
    }
}
