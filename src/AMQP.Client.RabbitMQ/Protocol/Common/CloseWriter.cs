using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class CloseWriter : IMessageWriter<CloseInfo>
    {
        private readonly ushort _channelId;
        private readonly short _classId;
        private readonly short _methodId;

        public CloseWriter(ushort channel, short classId, short methodId)
        {
            _classId = classId;
            _methodId = methodId;
            _channelId = channel;
        }

        public void WriteMessage(CloseInfo message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(_classId, _methodId, ref writer);
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
