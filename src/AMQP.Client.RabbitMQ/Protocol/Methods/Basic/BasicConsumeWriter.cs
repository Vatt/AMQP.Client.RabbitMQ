using System;
using System.Buffers;
using System.Buffers.Binary;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    internal class BasicConsumeWriter : IMessageWriter<ConsumeConf>
    {
        public void WriteMessage(ConsumeConf message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(RabbitMQConstants.FrameMethod);
            writer.WriteShortInt(message.ChannelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(60, 20, ref writer);
            writer.WriteShortInt(0); //reserved-1
            writer.WriteShortStr(message.QueueName);
            writer.WriteShortStr(message.ConsumerTag);
            writer.WriteBit(message.NoLocal);
            writer.WriteBit(message.NoAck);
            writer.WriteBit(message.Exclusive);
            writer.WriteBit(message.NoWait);
            writer.WriteTable(message.Arguments);
            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(RabbitMQConstants.FrameEnd);

            Span<byte> span = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(span, payloadSize);
            reserved.Write(span);

            writer.Commit();
        }
    }
}
