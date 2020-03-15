using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public static class ExchangeProtocolExtension
    {
        public static ValueTask SendExchangeDeclareAsync(this RabbitMQProtocol protocol, ushort channelId, ExchangeInfo message, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ExchangeDeclareWriter(channelId), message, token);
        }
        public static ValueTask<bool> ReadExchangeDeclareOk(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadNoPayload(token);
        }
        public static ValueTask SendExchangeDeleteAsync(this RabbitMQProtocol protocol, ushort channelId, ExchangeDeleteInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ExchangeDeleteWriter(channelId), info, token);
        }
        public static ValueTask<bool> ReadExchangeDeleteOk(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadNoPayload(token);
        }
    }
}
