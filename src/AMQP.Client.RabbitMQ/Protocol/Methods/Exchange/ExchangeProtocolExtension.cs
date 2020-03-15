using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public static class ExchangeProtocolExtension
    {
        public static ValueTask SendExchangeDeclareAsync(this RabbitMQProtocol protocol, ushort channelId, ExchangeInfo message)
        {
            return protocol.Writer.WriteAsync(new ExchangeDeclareWriter(channelId), message);
        }
        public static ValueTask<bool> ReadExchangeDeclareOk(this RabbitMQProtocol protocol)
        {
            return protocol.ReadNoPayload();
        }
        public static ValueTask SendExchangeDeleteAsync(this RabbitMQProtocol protocol, ushort channelId, ExchangeDeleteInfo info)
        {
            return protocol.Writer.WriteAsync(new ExchangeDeleteWriter(channelId), info);
        }
        public static ValueTask<bool> ReadExchangeDeleteOk(this RabbitMQProtocol protocol)
        {
            return protocol.ReadNoPayload();
        }
    }
}
