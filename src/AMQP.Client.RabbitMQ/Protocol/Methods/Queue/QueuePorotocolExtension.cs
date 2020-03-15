using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public static class QueuePorotocolExtension
    {
        private static readonly QueueDeclareOkReader _queueDeclareOkReader = new QueueDeclareOkReader();
        private static readonly QueuePurgeOkDeleteOkReader _queuePurgeOkDeleteOkReader = new QueuePurgeOkDeleteOkReader();
        public static ValueTask SendQueueDeclare(this RabbitMQProtocol protocol, ushort channelId, QueueInfo info)
        {
            return protocol.Writer.WriteAsync(new QueueDeclareWriter(channelId), info);
        }
        public static async ValueTask<QueueDeclareOk> ReadQueueDeclareOk(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_queueDeclareOkReader).ConfigureAwait(false);
            protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }
        public static ValueTask SendQueueBind(this RabbitMQProtocol protocol, ushort channelId, QueueBindInfo info)
        {
            return protocol.Writer.WriteAsync(new QueueBindWriter(channelId), info);
        }

        public static ValueTask SendQueueUnbind(this RabbitMQProtocol protocol, ushort channelId, QueueUnbindInfo info)
        {
            return protocol.Writer.WriteAsync(new QueueUnbindWriter(channelId), info);
        }
        public static ValueTask SendQueuePurge(this RabbitMQProtocol protocol, ushort channelId, QueuePurgeInfo info)
        {
            return protocol.Writer.WriteAsync(new QueuePurgeWriter(channelId), info);
        }
        public static ValueTask SendQueueDelete(this RabbitMQProtocol protocol, ushort channelId, QueueDeleteInfo info)
        {
            return protocol.Writer.WriteAsync(new QueueDeleteWriter(channelId), info);
        }
        public static async ValueTask<int> ReadQueuePurgeOkDeleteOk(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_queuePurgeOkDeleteOkReader).ConfigureAwait(false);
            protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }
        public static ValueTask<bool> ReadBindOkUnbindOk(this RabbitMQProtocol protocol)
        {
            return protocol.ReadNoPayload();
        }
    }
}
