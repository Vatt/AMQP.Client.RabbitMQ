using System;
using System.Buffers;
using System.Buffers.Binary;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public class ExchangeDeclareWriter : IMessageWriter<ExchangeInfo>
    {
        private readonly ushort _channelId;
        public ExchangeDeclareWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(ExchangeInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(40, 10, ref writer);
            writer.WriteShortInt(0); //reseved-1
            writer.WriteShortStr(message.Name);
            writer.WriteShortStr(message.Type);

            writer.WriteBit(message.Passive);
            writer.WriteBit(message.Durable);
            writer.WriteBit(message.AutoDelete);
            writer.WriteBit(message.Internal);
            writer.WriteBit(message.NoWait);
            writer.WriteTable(message.Arguments);
            var size = writer.Written - checkpoint;
            writer.WriteOctet(Constants.FrameEnd);

            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(sizeSpan, size);
            reserved.Write(sizeSpan);

            writer.Commit();
        }
    }
}
