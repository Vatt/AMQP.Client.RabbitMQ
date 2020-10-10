using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;

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
        private static readonly ByteWriter _byteWriter = new ByteWriter();
        public static ValueTask SendHeartbeat(this ProtocolWriter protocol, CancellationToken token = default)
        {
            return protocol.WriteAsync(_byteWriter, _heartbeatFrame, token);
        }
        public static ValueTask SendProtocol(this ProtocolWriter protocol, CancellationToken token = default)
        {
            return protocol.WriteAsync(_byteWriter, _protocolMsg, token);
        }
        /*
        public static ValueTask<FrameHeader> ReadFrameHeader(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_frameHeaderReader, token);
        }
        */
        public static MethodHeader ReadMethodHeader(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_methodHeaderReader, input);
        }
        public static bool ReadCloseOk(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return ReadNoPayload(protocol, input);
        }
        public static ValueTask<bool> ReadCloseOkAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return ReadNoPayloadAsync(protocol, token);
        }
        public static CloseInfo ReadClose(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_closeReader, input);
        }
        public static ValueTask<CloseInfo> ReadCloseAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_closeReader, token);
        }
        public static bool ReadNoPayload(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_noPayloadReader, input);
        }
        public static ValueTask<bool> ReadNoPayloadAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_noPayloadReader, token);
        }
        public static ValueTask SendClose(this ProtocolWriter protocol, CloseInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(ProtocolWriters.CloseWriter, info, token);
        }
        public static ValueTask SendCloseOk(this ProtocolWriter protocol, byte type, ushort channel, short classId, short methodId, CancellationToken token = default)
        {
            var info = new NoPaylodMethodInfo(type, channel, classId, methodId);
            return protocol.WriteAsync(new NoPayloadMethodWrtier(), info, token);
        }
        internal static string ReadShortStrPayload(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_shortStrPayloadReader, input);
        }
        internal static ValueTask<string> ReadShortStrPayloadAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_shortStrPayloadReader, token);
        }

    }
}
