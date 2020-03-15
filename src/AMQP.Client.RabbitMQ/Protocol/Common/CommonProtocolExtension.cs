using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public static class CommonProtocolExtension
    {
        private static readonly byte[] _protocolMsg = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        private static byte[] _heartbeatFrame => new byte[8] { 8, 0, 0, 0, 0, 0, 0, 206 };
        private static CloseReader _closeReader = new CloseReader();
        private static readonly MethodHeaderReader _methodHeaderReader = new MethodHeaderReader();
        private static readonly NoPayloadReader _noPayloadReader = new NoPayloadReader();
        private static readonly FrameHeaderReader _frameHeaderReader = new FrameHeaderReader();
        private static readonly ShortStrPayloadReader _shortStrPayloadReader = new ShortStrPayloadReader();
        public static ValueTask SendHeartbeat(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ByteWriter(), _heartbeatFrame, token);
        }
        public static ValueTask SendProtocol(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ByteWriter(), _protocolMsg, token);
        }
        public static ValueTask<FrameHeader> ReadFrameHeader(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_frameHeaderReader, token);
        }
        public static ValueTask<MethodHeader> ReadMethodHeader(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_methodHeaderReader, token);
        }
        public static ValueTask<bool> ReadCloseOk(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return ReadNoPayload(protocol, token);
        }

        public static ValueTask<CloseInfo> ReadClose(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_closeReader, token);
        }
        public static ValueTask<bool> ReadNoPayload(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_noPayloadReader, token);
        }
        public static ValueTask SendClose(this RabbitMQProtocol protocol, ushort channelId, short classId, short methodId, CloseInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new CloseWriter(channelId, classId, methodId), info, token);
        }
        public static ValueTask SendCloseOk(this RabbitMQProtocol protocol, byte type, ushort channel, short classId, short methodId, CancellationToken token = default)
        {
            var info = new NoPaylodMethodInfo(type, channel, classId, methodId);
            return protocol.WriteAsync(new NoPayloadMethodWrtier(), info, token);
        }
        internal static ValueTask<string> ReadShortStrPayload(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_shortStrPayloadReader, token);
        }

    }
}
