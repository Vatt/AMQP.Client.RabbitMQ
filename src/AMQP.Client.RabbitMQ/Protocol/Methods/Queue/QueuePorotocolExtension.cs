using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public static class QueuePorotocolExtension
    {
        private static readonly QueueDeclareOkReader _queueDeclareOkReader = new QueueDeclareOkReader();
        private static readonly QueuePurgeOkDeleteOkReader _queuePurgeOkDeleteOkReader = new QueuePurgeOkDeleteOkReader();
        public static ValueTask SendQueueDeclareAsync(this RabbitMQProtocolWriter protocol, ushort channelId, Queue info)
        {
            return protocol.WriteAsync(new QueueDeclareWriter(channelId), info);
        }
        public static QueueDeclareOk ReadQueueDeclareOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_queueDeclareOkReader, input);
        }
        public static ValueTask SendQueueBindAsync(this RabbitMQProtocolWriter protocol, ushort channelId, QueueBindInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new QueueBindWriter(channelId), info, token);
        }

        public static ValueTask SendQueueUnbindAsync(this RabbitMQProtocolWriter protocol, ushort channelId, QueueUnbindInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new QueueUnbindWriter(channelId), info, token);
        }
        public static ValueTask SendQueuePurgeAsync(this RabbitMQProtocolWriter protocol, ushort channelId, QueuePurgeInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new QueuePurgeWriter(channelId), info, token);
        }
        public static ValueTask SendQueueDeleteAsync(this RabbitMQProtocolWriter protocol, ushort channelId, QueueDelete info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new QueueDeleteWriter(channelId), info, token);
        }
        public static int ReadQueuePurgeOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_queuePurgeOkDeleteOkReader, input);
        }
        public static int ReadQueueDeleteOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_queuePurgeOkDeleteOkReader, input);
        }
        public static bool ReadQueueBindOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadNoPayload(input);
        }
        public static bool ReadQueueUnbindOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadNoPayload(input);
        }
    }
}
