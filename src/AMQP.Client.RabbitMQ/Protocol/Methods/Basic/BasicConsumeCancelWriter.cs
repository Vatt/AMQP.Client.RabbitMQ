using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    internal class BasicConsumeCancelWriter : IMessageWriter<ConsumeCancelInfo>
    {
        private readonly ushort _channelId;
        public BasicConsumeCancelWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(ConsumeCancelInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;

            FrameWriter.WriteMethodFrame(60, 30, ref writer);
            writer.WriteShortStr(message.ConsumerTag);
            writer.WriteBool(message.NoWait);
            var payloadSize = writer.Written - checkpoint;
            writer.WriteOctet(Constants.FrameEnd);

            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(sizeSpan, 18);
            reserved.Write(sizeSpan);

            writer.Commit();
        }
    }
}
