using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public class ContentHeaderWriter : IMessageWriter<ContentHeader>
    {
        private readonly ushort _channelId;
        private int m_bitCount;
        private ushort m_flagWord;
        public ContentHeaderWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(ContentHeader message, IBufferWriter<byte> output)
        {
            m_bitCount = 0;
            m_flagWord = 0;
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(Constants.FrameHeader);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            writer.WriteShortInt(message.ClassId);
            writer.WriteShortInt(message.Weight);
            writer.WriteLongLong(message.BodySize);
            
            WriteBitFlagsAndContinuation(message, ref writer);


            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(Constants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            writer.Commit();
        }
        private void WriteBitFlagsAndContinuation(ContentHeader message,ref ValueWriter writer)
        {
            if (message.ContentType != null) { WritePresence(true, ref writer); }
            if (message.ContentEncoding != null) { WritePresence(true, ref writer); }
            if (message.Headers != null) { WritePresence(true, ref writer); }
            if (message.DeliveryMode != 0) { WritePresence(true, ref writer); }
            if (message.Priority != 0) { WritePresence(true, ref writer); }
            if (message.CorrelationId != null) { WritePresence(true, ref writer); }
            if (message.ReplyTo != null) { WritePresence(true, ref writer); }
            if (message.Expiration != null) { WritePresence(true, ref writer); }
            if (message.MessageId != null) { WritePresence(true, ref writer); }
            if (message.Timestamp != 0) { WritePresence(true, ref writer); }
            if (message.Type != null) { WritePresence(true, ref writer); }
            if (message.UserId != null) { WritePresence(true, ref writer); }
            if (message.AppId != null) { WritePresence(true, ref writer); }
            if (message.ClusterId != null) { WritePresence(true, ref writer); }
            writer.WriteShortInt(m_flagWord);
            if (message.ContentType != null) { writer.WriteShortStr(message.ContentType); }
            if (message.ContentEncoding != null) { writer.WriteShortStr(message.ContentEncoding); }
            if (message.Headers != null) { writer.WriteTable(message.Headers); }
            if (message.DeliveryMode != 0) { writer.WriteOctet(message.DeliveryMode); }
            if (message.Priority != 0) { writer.WriteOctet(message.Priority); }
            if (message.CorrelationId != null) { writer.WriteShortStr(message.CorrelationId); }
            if (message.ReplyTo != null) { writer.WriteShortStr(message.ReplyTo); }
            if (message.Expiration != null) { writer.WriteShortStr(message.Expiration); }
            if (message.MessageId != null) { writer.WriteShortStr(message.MessageId); }
            if (message.Timestamp != 0) { writer.WriteLongLong(message.Timestamp); }
            if (message.Type != null) { writer.WriteShortStr(message.Type); }
            if (message.UserId != null) { writer.WriteShortStr(message.UserId); }
            if (message.AppId != null) { writer.WriteShortStr(message.AppId); }
            if (message.ClusterId != null) { writer.WriteShortStr(message.ClusterId); }
        }
        private void WritePresence(bool present, ref ValueWriter writer)
        {
            //if (m_bitCount == 15)
            //{
            //    EmitFlagWord(true,ref writer);
            //}

            if (present)
            {
                int bit = 15 - m_bitCount;
                m_flagWord = (ushort)(m_flagWord | (1 << bit));
            }
            m_bitCount++;
        }
        private void EmitFlagWord(bool continuationBit, ref ValueWriter writer)
        {
            writer.WriteShortInt((ushort)(continuationBit ? (m_flagWord | 1) : m_flagWord));
            m_flagWord = 0;
            m_bitCount = 0;
        }
    }
}
