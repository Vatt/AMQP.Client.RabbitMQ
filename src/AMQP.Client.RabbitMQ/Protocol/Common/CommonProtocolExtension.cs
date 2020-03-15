using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using System;
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
        public static ValueTask SendHeartbeat(this RabbitMQProtocol protocol)
        {
            return protocol.Writer.WriteAsync(new ByteWriter(), _heartbeatFrame);
        }
        public static ValueTask SendProtocol(this RabbitMQProtocol protocol)
        {
            return protocol.Writer.WriteAsync(new ByteWriter(), _protocolMsg);
        }
        public static async ValueTask<FrameHeader> ReadFrameHeader(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            var result = await protocol.Reader.ReadAsync(_frameHeaderReader, token).ConfigureAwait(false);
            protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:  сделать чтонибудь
            }
            return result.Message;
        }
        public static async ValueTask<MethodHeader> ReadMethodHeader(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_methodHeaderReader).ConfigureAwait(false);
            protocol.Reader.Advance();
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            return result.Message;
        }
        public static async ValueTask<bool> ReadCloseOk(this RabbitMQProtocol protocol)
        {
            return await ReadNoPayload(protocol);
        }

        public static async ValueTask<CloseInfo> ReadClose(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_closeReader).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            protocol.Reader.Advance();
            return result.Message;
        }
        public static async ValueTask<bool> ReadNoPayload(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_noPayloadReader).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            protocol.Reader.Advance();
            return result.Message;
        }
        public static ValueTask SendClose(this RabbitMQProtocol protocol, ushort channelId, short classId, short methodId, CloseInfo info)
        {
            return protocol.Writer.WriteAsync(new CloseWriter(channelId, classId, methodId), info);
        }
        public static ValueTask SendCloseOk(this RabbitMQProtocol protocol, byte type, ushort channel, short classId, short methodId)
        {
            var info = new NoPaylodMethodInfo(type, channel, classId, methodId);
            return protocol.Writer.WriteAsync(new NoPayloadMethodWrtier(), info);
        }
        internal static async ValueTask<string> ReadShortStrPayload(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_shortStrPayloadReader).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            protocol.Reader.Advance();
            return result.Message;
        }

        public static async ValueTask<ContentHeader> ReadContentHeaderWithFrameHeader(this RabbitMQProtocol protocol, ushort channelId)
        {
            var reader = new ContentHeaderFullReader(channelId);
            var result = await protocol.Reader.ReadAsync(reader).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            protocol.Reader.Advance();
            return result.Message;
        }

        public static ValueTask PublishAll(this RabbitMQProtocol protocol, ushort channelId, PublishFullContent content)
        {
            return protocol.Writer.WriteAsync(new PublishFullWriter(channelId), content);
        }
        public static ValueTask PublishPartial(this RabbitMQProtocol protocol, ushort channelId, PublishInfoAndContent infoAndContent)
        {
            var writer = new PublishInfoAndContentWriter(channelId);
            return protocol.Writer.WriteAsync(writer, infoAndContent);
        }
        public static ValueTask PublishBody(this RabbitMQProtocol protocol, ushort channelId, ReadOnlyMemory<byte>[] batch)
        {
            return protocol.Writer.WriteManyAsync(new BodyFrameWriter(channelId), batch);
        }
    }
}
