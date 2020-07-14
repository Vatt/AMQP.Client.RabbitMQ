using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    //TODO: вот это подзамену с механикой вычитки пайлоада сразу
    internal class ContentHeaderFullReader : IMessageReader<ContentHeader>
    {
        private readonly ushort _channel;
        private ushort _bitCount = 0;
        public ContentHeaderFullReader(ushort channelId)
        {
            _channel = channelId;
        }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ContentHeader message)
        {
            message = default;
            _bitCount = 0;
            ValueReader reader = new ValueReader(input, consumed);

            if (!reader.ReadOctet(out var type)) { return false; }
            if (!reader.ReadShortInt(out short channel)) { return false; }
            if (!reader.ReadLong(out int payload)) { return false; }
            if (type != Constants.FrameHeader && channel != _channel)
            {
                throw new Exception($"Missmatch FrameType or Channel in{typeof(ContentHeaderFullReader)}");
            }

            if (!reader.ReadShortInt(out short classId)) { return false; }
            if (!reader.ReadShortInt(out short weight)) { return false; }
            if (!reader.ReadLongLong(out long bodySize)) { return false; }
            if (!reader.ReadShortInt(out short propertyFlags)) { return false; }
            message = new ContentHeader((ushort)classId, (ushort)weight, bodySize);
            if (!ReadBitFlagsAndContinuation((ushort)propertyFlags, ref reader, ref message.Properties))
            {
                return false;
            }

            if (!reader.ReadOctet(out var endMarker))
            {
                return false;
            }
            if (endMarker != Constants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            consumed = reader.Position;
            examined = consumed;
            return true;

        }
        private bool ReadBitFlagsAndContinuation(ushort flags, ref ValueReader reader,
                                                 ref ContentHeaderProperties properties)
        {
            var m_contentType_present = ReadPresence(flags);
            var m_contentEncoding_present = ReadPresence(flags);
            var m_headers_present = ReadPresence(flags);
            var m_deliveryMode_present = ReadPresence(flags);
            var m_priority_present = ReadPresence(flags);
            var m_correlationId_present = ReadPresence(flags);
            var m_replyTo_present = ReadPresence(flags);
            var m_expiration_present = ReadPresence(flags);
            var m_messageId_present = ReadPresence(flags);
            var m_timestamp_present = ReadPresence(flags);
            var m_type_present = ReadPresence(flags);
            var m_userId_present = ReadPresence(flags);
            var m_appId_present = ReadPresence(flags);
            var m_clusterId_present = ReadPresence(flags);

            if (m_contentType_present)
            {
                if (!reader.ReadShortStr(out var m_contentType)) { return false; }
                properties.ContentType = m_contentType;
            }
            if (m_contentEncoding_present)
            {
                if (!reader.ReadShortStr(out var m_contentEncoding)) { return false; }
                properties.ContentEncoding = m_contentEncoding;
            }
            if (m_headers_present)
            {
                if (!reader.ReadTable(out var m_headers)) { return false; }
                properties.Headers = m_headers;
            }
            if (m_deliveryMode_present)
            {
                if (!reader.ReadOctet(out var m_deliveryMode)) { return false; }
                properties.DeliveryMode = m_deliveryMode;
            }
            if (m_priority_present)
            {
                if (!reader.ReadOctet(out var m_priority)) { return false; }
                properties.Priority = m_priority;
            }
            if (m_correlationId_present)
            {
                if (!reader.ReadShortStr(out var m_correlationId)) { return false; }
                properties.CorrelationId = m_correlationId;
            }
            if (m_replyTo_present)
            {
                if (!reader.ReadShortStr(out var m_replyTo)) { return false; }
                properties.ReplyTo = m_replyTo;
            }
            if (m_expiration_present)
            {
                if (!reader.ReadShortStr(out var m_expiration)) { return false; }
                properties.Expiration = m_expiration;
            }
            if (m_messageId_present)
            {
                if (!reader.ReadShortStr(out var m_messageId)) { return false; }
                properties.MessageId = m_messageId;
            }
            if (m_timestamp_present)
            {
                if (!reader.ReadTimestamp(out var m_timestamp)) { return false; }
                properties.Timestamp = m_timestamp;
            }
            if (m_type_present)
            {
                if (!reader.ReadShortStr(out var m_type)) { return false; }
                properties.Type = m_type;
            }
            if (m_userId_present)
            {
                if (!reader.ReadShortStr(out var m_userId)) { return false; }
                properties.UserId = m_userId;
            }
            if (m_appId_present)
            {
                if (!reader.ReadShortStr(out var m_appId)) { return false; }
                properties.AppId = m_appId;
            }
            if (m_clusterId_present)
            {
                if (!reader.ReadShortStr(out var m_clusterId)) { return false; }
                properties.ClusterId = m_clusterId;
            }

            return true;
        }
        public bool ReadPresence(ushort flags)
        {
            //if (m_bitCount == 15)
            //{
            //    ReadFlagWord();
            //}

            int bit = 15 - _bitCount;
            bool result = (flags & (1 << bit)) != 0;
            _bitCount++;
            return result;
        }
    }
}
