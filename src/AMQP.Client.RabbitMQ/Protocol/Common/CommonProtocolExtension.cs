using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public static class CommonProtocolExtension
    {
        private static readonly ReadOnlyMemory<byte> _protocolMsg = new ReadOnlyMemory<byte>(new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 });
        private static readonly ReadOnlyMemory<byte> _heartbeatFrame = new ReadOnlyMemory<byte>(new byte[8] { 8, 0, 0, 0, 0, 0, 0, 206 });
        private static readonly CloseReader _closeReader = new CloseReader();
        private static readonly MethodHeaderReader _methodHeaderReader = new MethodHeaderReader();
        private static readonly NoPayloadReader _noPayloadReader = new NoPayloadReader();
        private static readonly FrameHeaderReader _frameHeaderReader = new FrameHeaderReader();
        private static readonly ShortStrPayloadReader _shortStrPayloadReader = new ShortStrPayloadReader();
        public static ValueTask SendHeartbeat(this RabbitMQProtocolWriter protocol, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ByteWriter(), _heartbeatFrame, token);
        }
        public static ValueTask SendProtocol(this RabbitMQProtocolWriter protocol, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ByteWriter(), _protocolMsg, token);
        }
        public static ValueTask<FrameHeader> ReadFrameHeader(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_frameHeaderReader, token);
        }
        public static MethodHeader ReadMethodHeader(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_methodHeaderReader, input);
        }
        public static bool ReadCloseOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return ReadNoPayload(protocol, input);
        }

        public static CloseInfo ReadClose(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_closeReader, input);
        }
        public static bool ReadNoPayload(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_noPayloadReader, input);
        }
        public static ValueTask SendClose(this RabbitMQProtocolWriter protocol, ushort channelId, short classId, short methodId, CloseInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new CloseWriter(channelId, classId, methodId), info, token);
        }
        public static ValueTask SendCloseOk(this RabbitMQProtocolWriter protocol, byte type, ushort channel, short classId, short methodId, CancellationToken token = default)
        {
            var info = new NoPaylodMethodInfo(type, channel, classId, methodId);
            return protocol.WriteAsync(new NoPayloadMethodWrtier(), info, token);
        }
        internal static string ReadShortStrPayload(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_shortStrPayloadReader, input);
        }

    }
}
