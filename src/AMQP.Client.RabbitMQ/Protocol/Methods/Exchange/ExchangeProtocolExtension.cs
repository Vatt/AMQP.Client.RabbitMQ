using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Core;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public static class ExchangeProtocolExtension
    {
        public static ValueTask SendExchangeDeclareAsync(this ProtocolWriter protocol, ushort channelId, ExchangeDeclare message, CancellationToken token = default)
        {
            return protocol.WriteAsync(ProtocolWriters.ExchangeDeclareWriter, message, token);
        }
        public static bool ReadExchangeDeclareOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadNoPayload(input);
        }
        public static ValueTask<bool> ReadExchangeDeclareOkAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadNoPayloadAsync(token);
        }
        public static ValueTask SendExchangeDeleteAsync(this ProtocolWriter protocol, ushort channelId, ExchangeDelete info, CancellationToken token = default)
        {
            return protocol.WriteAsync(ProtocolWriters.ExchangeDeleteWriter, info, token);
        }
        public static bool ReadExchangeDeleteOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadNoPayload(input);
        }
        public static ValueTask<bool> ReadExchangeDeleteOkAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadNoPayloadAsync(token);
        }
    }
}
