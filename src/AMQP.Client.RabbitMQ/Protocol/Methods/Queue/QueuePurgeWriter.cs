using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public class QueuePurgeWriter : IMessageWriter<QueuePurgeInfo>
    {
        private readonly ushort _channelId;
        public QueuePurgeWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(QueuePurgeInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(Constants.FrameMethod);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(50, 30, ref writer);
            writer.WriteShortInt(0); //reserved-1
            writer.WriteShortStr(message.QueueName);
            writer.WriteBit(message.NoWait);
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
