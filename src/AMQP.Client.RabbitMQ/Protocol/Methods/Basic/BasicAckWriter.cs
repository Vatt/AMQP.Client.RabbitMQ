using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public class BasicAckWriter : IMessageWriter<AckInfo>
    {
        public readonly ushort _channelId;
        public BasicAckWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(AckInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(Constants.FrameMethod);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(60, 80, ref writer);
            writer.WriteLongLong(message.DeliveryTag);
            writer.WriteBit(message.Multiple);
            writer.BitFlush();
            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(Constants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            writer.Commit();

        }
    }
}
