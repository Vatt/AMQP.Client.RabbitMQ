using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Core;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Channel
{
    public static class ChannelProtocolExtension
    {
        private static readonly ChannelOpenWriter _channelOpenWriter = new ChannelOpenWriter();
        private static readonly ChannelOpenOkReader channelOpenOkReader = new ChannelOpenOkReader();
        public static ValueTask SendChannelOpenAsync(this ProtocolWriter protocol, ushort channelId, CancellationToken token = default)
        {
            return protocol.WriteAsync(_channelOpenWriter, channelId, token);
        }
        public static bool ReadChannelOpenOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(channelOpenOkReader, input);
        }
        public static ValueTask<bool> ReadChannelOpenOkAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(channelOpenOkReader, token);
        }

    }
}
