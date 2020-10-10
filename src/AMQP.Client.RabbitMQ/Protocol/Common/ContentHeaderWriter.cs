using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class ContentHeaderWriter : IMessageWriter<ContentHeader>
    {
        private int _bitCount;
        private ushort _flagWord;

        public void WriteMessage(ContentHeader message, IBufferWriter<byte> output)
        {
            _bitCount = 0;
            _flagWord = 0;
            var writer = new ValueWriter(output);
            writer.WriteOctet(RabbitMQConstants.FrameHeader);
            writer.WriteShortInt(message.ChannelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            writer.WriteShortInt(message.ClassId);
            writer.WriteShortInt(message.Weight);
            writer.WriteLongLong(message.BodySize);

            WriteBitFlagsAndContinuation(ref message.Properties, ref writer);


            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(RabbitMQConstants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            writer.Commit();
        }

        internal void WriteMessage(ref ContentHeader message, ref ValueWriter writer)
        {
            _bitCount = 0;
            _flagWord = 0;
            writer.WriteOctet(RabbitMQConstants.FrameHeader);
            writer.WriteShortInt(message.ChannelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            writer.WriteShortInt(message.ClassId);
            writer.WriteShortInt(message.Weight);
            writer.WriteLongLong(message.BodySize);

            WriteBitFlagsAndContinuation(ref message.Properties, ref writer);


            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(RabbitMQConstants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            writer.Commit();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteBitFlagsAndContinuation(ref ContentHeaderProperties properties, ref ValueWriter writer)
        {
            WritePresence(properties.ContentType != null);
            WritePresence(properties.ContentEncoding != null);
            WritePresence(properties.Headers != null);
            WritePresence(properties.DeliveryMode != 0);
            WritePresence(properties.Priority != 0);
            WritePresence(properties.CorrelationId != null);
            WritePresence(properties.ReplyTo != null);
            WritePresence(properties.Expiration != null);
            WritePresence(properties.MessageId != null);
            WritePresence(properties.Timestamp != 0);
            WritePresence(properties.Type != null);
            WritePresence(properties.UserId != null);
            WritePresence(properties.AppId != null);
            WritePresence(properties.ClusterId != null);
            writer.WriteShortInt(_flagWord);
            if (properties.ContentType != null)
            {
                writer.WriteShortStr(properties.ContentType);
            }

            if (properties.ContentEncoding != null)
            {
                writer.WriteShortStr(properties.ContentEncoding);
            }

            if (properties.Headers != null)
            {
                writer.WriteTable(properties.Headers);
            }

            if (properties.DeliveryMode != 0)
            {
                writer.WriteOctet(properties.DeliveryMode);
            }

            if (properties.Priority != 0)
            {
                writer.WriteOctet(properties.Priority);
            }

            if (properties.CorrelationId != null)
            {
                writer.WriteShortStr(properties.CorrelationId);
            }

            if (properties.ReplyTo != null)
            {
                writer.WriteShortStr(properties.ReplyTo);
            }

            if (properties.Expiration != null)
            {
                writer.WriteShortStr(properties.Expiration);
            }

            if (properties.MessageId != null)
            {
                writer.WriteShortStr(properties.MessageId);
            }

            if (properties.Timestamp != 0)
            {
                writer.WriteLongLong(properties.Timestamp);
            }

            if (properties.Type != null)
            {
                writer.WriteShortStr(properties.Type);
            }

            if (properties.UserId != null)
            {
                writer.WriteShortStr(properties.UserId);
            }

            if (properties.AppId != null)
            {
                writer.WriteShortStr(properties.AppId);
            }

            if (properties.ClusterId != null)
            {
                writer.WriteShortStr(properties.ClusterId);
            }
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