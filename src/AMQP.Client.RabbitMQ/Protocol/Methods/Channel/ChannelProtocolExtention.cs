using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Channel
{
    public static class ChannelProtocolExtention
    {
        private static readonly ChannelOpenWriter _channelOpenWriter = new ChannelOpenWriter();
        private static readonly ChannelOpenOkReader channelOpenOkReader = new ChannelOpenOkReader();
        public static ValueTask SendChannelOpen(this RabbitMQProtocol protocol, ushort channelId)
        {
            return protocol.Writer.WriteAsync(_channelOpenWriter, channelId);
        }
        public static async ValueTask ReadChannelOpenOk(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(channelOpenOkReader).ConfigureAwait(false);
            protocol.Reader.Advance();
        }

    }
}
