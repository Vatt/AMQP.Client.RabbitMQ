using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public class ContentHeaderFullReader : IMessageReader<ContentHeader>
    {
        private readonly ushort _channel; 
        private ushort m_bitCount = 0;
        public ContentHeaderFullReader(ushort channelId)
        {
            _channel = channelId;
        }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ContentHeader message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            
            if(!reader.ReadOctet(out var type)) { return false; }
            if(!reader.ReadShortInt(out ushort channel)) { return false; }
            if(!reader.ReadLong(out int payload)) { return false; }
            if(type != Constants.FrameHeader && channel != _channel)
            {
                throw new Exception($"Missmatch FrameType ot Channel in{typeof(ContentHeaderFullReader)}");
            }

            if(!reader.ReadShortInt(out short classId)) { return false; }
            if(!reader.ReadShortInt(out short weight)) { return false; }
            if(!reader.ReadLongLong(out long bodySize)) { return false; }
            if(!reader.ReadShortInt(out short propertyFlags)) { return false; }
            message = new ContentHeader((ushort)classId, (ushort)weight, bodySize);
            if(!ReadBitFlagsAndContinuation((ushort)propertyFlags,ref reader, ref message.ContentType, ref message.ContentEncoding, ref message.Headers,
                                            ref message.DeliveryMode, ref message.Priority, ref message.CorrelationId, ref message.ReplyTo,
                                            ref message.Expiration, ref message.MessageId, ref message.Timestamp, ref message.Type, ref message.UserId,
                                            ref message.AppId, ref message.ClusterId))
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
                                                 ref string m_contentType, ref string m_contentEncoding,ref Dictionary<string, object> m_headers,
                                                 ref byte m_deliveryMode, ref byte m_priority, ref string m_correlationId, ref string m_replyTo,
                                                 ref string m_expiration, ref string m_messageId, ref long m_timestamp,ref string m_type,
                                                 ref string m_userId, ref string m_appId, ref string m_clusterId)
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

            if (m_contentType_present) { if (!reader.ReadShortStr(out m_contentType)) { return false; } }
            if (m_contentEncoding_present) { if (!reader.ReadShortStr(out m_contentEncoding)) { return false; } }
            if (m_headers_present) { if (!reader.ReadTable(out m_headers)) { return false; } }
            if (m_deliveryMode_present) { if (!reader.ReadOctet(out m_deliveryMode)) { return false; } }
            if (m_priority_present) { if (!reader.ReadOctet(out m_priority)) { return false; } }
            if (m_correlationId_present) { if (!reader.ReadShortStr(out m_correlationId)) { return false; } }
            if (m_replyTo_present) { if (!reader.ReadShortStr(out m_replyTo)) { return false; } }
            if (m_expiration_present) { if (!reader.ReadShortStr(out m_expiration)) { return false; } }
            if (m_messageId_present) { if (!reader.ReadShortStr(out m_messageId)) { return false; } }
            if (m_timestamp_present) { if (!reader.ReadTimestamp(out m_timestamp)) { return false; } }
            if (m_type_present) { if (!reader.ReadShortStr(out m_type)) { return false; } }
            if (m_userId_present) { if (!reader.ReadShortStr(out m_userId)) { return false; } }
            if (m_appId_present) { if (!reader.ReadShortStr(out m_appId)) { return false; } }
            if (m_clusterId_present) { if (!reader.ReadShortStr(out m_clusterId)) { return false; } }

            return true;
        }
        public bool ReadPresence(ushort flags)
        {
            //if (m_bitCount == 15)
            //{
            //    ReadFlagWord();
            //}

            int bit = 15 - m_bitCount;
            bool result = (flags & (1 << bit)) != 0;
            m_bitCount++;
            return result;
        }
    }
}
