using System;
using System.Buffers;
using System.Buffers.Binary;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    internal class ExchangeDeleteWriter : IMessageWriter<ExchangeDelete>
    {
        public void WriteMessage(ExchangeDelete message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(message.ChannelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(40, 20, ref writer);
            writer.WriteShortInt(0);
            writer.WriteShortStr(message.Name);
            writer.WriteBit(message.IfUnused);
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
