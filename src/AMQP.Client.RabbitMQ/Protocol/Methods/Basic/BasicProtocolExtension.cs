using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public static class BasicProtocolExtension
    {
        private static readonly BasicDeliverReader _basicDeliverReader = new BasicDeliverReader();
        public static ValueTask SendBasicConsume(this RabbitMQProtocol protocol, ushort channelId, ConsumerInfo info)
        {
            return protocol.Writer.WriteAsync(new BasicConsumeWriter(channelId), info);
        }
        public static ValueTask<string> ReadBasicConsumeOk(this RabbitMQProtocol protocol)
        {
            return protocol.ReadShortStrPayload();
        }
        public static async ValueTask<DeliverInfo> ReadBasicDeliver(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_basicDeliverReader).ConfigureAwait(false);
            protocol.Reader.Advance();
            if (!result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            return result.Message;

        }
        public static ValueTask SendBasicQoS(this RabbitMQProtocol protocol, ushort channelId, ref QoSInfo info)
        {
            return protocol.Writer.WriteAsync(new BasicQoSWriter(channelId), info);
        }
        public static ValueTask<bool> ReadBasicQoSOk(this RabbitMQProtocol protocol)
        {
            return protocol.ReadNoPayload();
        }
    }
}
