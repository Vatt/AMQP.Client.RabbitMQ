using System;
using System.Buffers;
using System.Buffers.Binary;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{

    public class QueueDeclareWriter : IMessageWriter<QueueInfo>
    {
        private readonly ushort ChannelId;
        public QueueDeclareWriter(ushort channelId)
        {
            ChannelId = channelId;
        }
        public void WriteMessage(QueueInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(ChannelId);
            var reseved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(50, 10, ref writer);
            writer.WriteShortInt(0); //reserved-1
            writer.WriteShortStr(message.Name);
            writer.WriteBit(message.Passive);
            writer.WriteBit(message.Durable);
            writer.WriteBit(message.Exclusive);
            writer.WriteBit(message.AutoDelete);
            writer.WriteBit(message.NoWait);
            writer.WriteTable(message.Arguments);
            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(Constants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reseved.Write(span);

            writer.Commit();
        }
    }
}
