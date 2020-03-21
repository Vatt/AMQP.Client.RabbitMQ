using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class ContentHeaderWriter : IMessageWriter<ContentHeader>
    {
        private readonly ushort _channelId;
        private int _bitCount;
        private ushort _flagWord;
        public ContentHeaderWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(ContentHeader message, IBufferWriter<byte> output)
        {
            _bitCount = 0;
            _flagWord = 0;
            var writer = new ValueWriter(output);
            writer.WriteOctet(Constants.FrameHeader);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            writer.WriteShortInt(message.ClassId);
            writer.WriteShortInt(message.Weight);
            writer.WriteLongLong(message.BodySize);

            WriteBitFlagsAndContinuation(ref message.Properties, ref writer);


            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(Constants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            writer.Commit();
        }
        internal void WriteMessage(ref ContentHeader message, ref ValueWriter writer)
        {
            _bitCount = 0;
            _flagWord = 0;
            writer.WriteOctet(Constants.FrameHeader);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            writer.WriteShortInt(message.ClassId);
            writer.WriteShortInt(message.Weight);
            writer.WriteLongLong(message.BodySize);

            WriteBitFlagsAndContinuation(ref message.Properties, ref writer);


            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(Constants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

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
}
