using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public class ExchangeDeleteWriter : IMessageWriter<ExchangeDeleteInfo>
    {
        private readonly ushort _channelId;
        public ExchangeDeleteWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(ExchangeDeleteInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(_channelId);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(40, 20, ref writer);
            writer.WriteShortInt(0);
            writer.WriteShortStr(message.Name);
            writer.WriteBit(message.IfUnused);
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
