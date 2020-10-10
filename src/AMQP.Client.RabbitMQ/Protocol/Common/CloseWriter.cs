using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class CloseWriter : IMessageWriter<CloseInfo>
    {
        public void WriteMessage(CloseInfo message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(message.ChannelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(message.ClassId, message.MethodId, ref writer);
            writer.WriteShortInt(message.ReplyCode);
            writer.WriteShortStr(message.ReplyText);
            writer.WriteShortInt(message.FailedClassId);
            writer.WriteShortInt(message.FailedMethodId);
            var size = writer.Written - checkpoint;
            writer.WriteOctet(206);
            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(sizeSpan, size);
            reserved.Write(sizeSpan);
            writer.Commit();
        }
    }
}
