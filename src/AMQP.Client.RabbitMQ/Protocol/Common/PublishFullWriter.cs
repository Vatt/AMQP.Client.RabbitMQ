using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class PublishFullWriter : IMessageWriter<PublishAllInfo>
    {
        private int _bitCount;
        private ushort _flagWord;

        public PublishFullWriter()
        {

        }

        public void WriteMessage(PublishAllInfo message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);

            var framePayloadSize = 9 + message.Info.ExchangeName.Length + message.Info.RoutingKey.Length;
            FrameWriter.WriteFrameHeader(RabbitMQConstants.FrameMethod, message.ChannelId, framePayloadSize, ref writer);
            FrameWriter.WriteMethodFrame(60, 40, ref writer);
            writer.WriteShortInt(0); //reserved-1
            writer.WriteShortStr(message.Info.ExchangeName);
            writer.WriteShortStr(message.Info.RoutingKey);
            writer.WriteBit(message.Info.Mandatory);
            writer.WriteBit(message.Info.Immediate);
            writer.BitFlush();
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
            
            _bitCount = 0;
            _flagWord = 0;
            writer.WriteOctet(RabbitMQConstants.FrameHeader);
            writer.WriteShortInt(message.ChannelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            writer.WriteShortInt(message.Header.ClassId);
            writer.WriteShortInt(message.Header.Weight);
            writer.WriteLongLong(message.Header.BodySize);
            WriteBitFlagsAndContinuation(ref message.Header.Properties, ref writer);
            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            FrameWriter.WriteFrameHeader(RabbitMQConstants.FrameBody, message.ChannelId, message.Body.Length, ref writer);
            writer.WriteBytes(message.Body.Span);
            writer.WriteOctet(RabbitMQConstants.FrameEnd);


            writer.Commit();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteBitFlagsAndContinuation(ref ContentHeaderProperties properties, ref ValueWriter writer)
        {
            if (properties.ContentType != null) { WritePresence(true); }
            if (properties.ContentEncoding != null) { WritePresence(true); }
            if (properties.Headers != null) { WritePresence(true); }
            if (properties.DeliveryMode != 0) { WritePresence(true); }
            if (properties.Priority != 0) { WritePresence(true); }
            if (properties.CorrelationId != null) { WritePresence(true); }
            if (properties.ReplyTo != null) { WritePresence(true); }
            if (properties.Expiration != null) { WritePresence(true); }
            if (properties.MessageId != null) { WritePresence(true); }
            if (properties.Timestamp != 0) { WritePresence(true); }
            if (properties.Type != null) { WritePresence(true); }
            if (properties.UserId != null) { WritePresence(true); }
            if (properties.AppId != null) { WritePresence(true); }
            if (properties.ClusterId != null) { WritePresence(true); }
            writer.WriteShortInt(_flagWord);
            if (properties.ContentType != null) { writer.WriteShortStr(properties.ContentType); }
            if (properties.ContentEncoding != null) { writer.WriteShortStr(properties.ContentEncoding); }
            if (properties.Headers != null) { writer.WriteTable(properties.Headers); }
            if (properties.DeliveryMode != 0) { writer.WriteOctet(properties.DeliveryMode); }
            if (properties.Priority != 0) { writer.WriteOctet(properties.Priority); }
            if (properties.CorrelationId != null) { writer.WriteShortStr(properties.CorrelationId); }
            if (properties.ReplyTo != null) { writer.WriteShortStr(properties.ReplyTo); }
            if (properties.Expiration != null) { writer.WriteShortStr(properties.Expiration); }
            if (properties.MessageId != null) { writer.WriteShortStr(properties.MessageId); }
            if (properties.Timestamp != 0) { writer.WriteLongLong(properties.Timestamp); }
            if (properties.Type != null) { writer.WriteShortStr(properties.Type); }
            if (properties.UserId != null) { writer.WriteShortStr(properties.UserId); }
            if (properties.AppId != null) { writer.WriteShortStr(properties.AppId); }
            if (properties.ClusterId != null) { writer.WriteShortStr(properties.ClusterId); }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WritePresence(bool present)
        {
            if (present)
            {
                int bit = 15 - _bitCount;
                _flagWord = (ushort)(_flagWord | (1 << bit));
            }
            _bitCount++;
        }
    }

    public class PublishAllInfo
    {
        public ContentHeader _contentHeader;
        public BasicPublishInfo _info;        
        public ushort ChannelId;
        public ReadOnlyMemory<byte> Body { get; }

        public PublishAllInfo(ushort channelId, ref ReadOnlyMemory<byte> body, ref BasicPublishInfo info, ContentHeader header)
        {
            Body = body;
            _info = info;
            _contentHeader = header;
            ChannelId = channelId;
        }

        public ref BasicPublishInfo Info => ref  _info;
        public ref ContentHeader Header => ref  _contentHeader;
    }
}
