using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    internal class QueueDeleteWriter : IMessageWriter<QueueDelete>
    {
        private readonly ushort _channelId;
        public QueueDeleteWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(QueueDelete message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(RabbitMQConstants.FrameMethod);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(50, 40, ref writer);
            writer.WriteShortInt(0); //reserved-1
            writer.WriteShortStr(message.Name);
            writer.WriteBit(message.IfUnused);
            writer.WriteBit(message.IfEmpty);
            writer.WriteBit(message.NoWait);
            writer.BitFlush();
            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(RabbitMQConstants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            writer.Commit();
        }
    }
}
