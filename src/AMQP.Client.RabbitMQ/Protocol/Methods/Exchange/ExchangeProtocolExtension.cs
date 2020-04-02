using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public static class ExchangeProtocolExtension
    {
        public static ValueTask SendExchangeDeclareAsync(this RabbitMQProtocolWriter protocol, ushort channelId, Exchange message, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ExchangeDeclareWriter(channelId), message, token);
        }
        public static bool ReadExchangeDeclareOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadNoPayload(input);
        }
        public static ValueTask SendExchangeDeleteAsync(this RabbitMQProtocolWriter protocol, ushort channelId, ExchangeDelete info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ExchangeDeleteWriter(channelId), info, token);
        }
        public static bool ReadExchangeDeleteOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.ReadNoPayload(input);
        }
    }
}
