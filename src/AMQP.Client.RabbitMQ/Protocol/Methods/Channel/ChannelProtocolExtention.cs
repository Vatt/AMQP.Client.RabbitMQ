using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Channel
{
    public static class ChannelProtocolExtention
    {
        private static readonly ChannelOpenWriter _channelOpenWriter = new ChannelOpenWriter();
        private static readonly ChannelOpenOkReader channelOpenOkReader = new ChannelOpenOkReader();
        public static ValueTask SendChannelOpen(this RabbitMQProtocol protocol, ushort channelId, CancellationToken token = default)
        {
            return protocol.WriteAsync(_channelOpenWriter, channelId, token);
        }
        public static ValueTask<bool> ReadChannelOpenOk(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(channelOpenOkReader, token);
        }

    }
}
