using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    internal class BasicAckWriter : IMessageWriter<AckInfo>
    {
        public readonly ushort _channelId;
        public BasicAckWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(AckInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(RabbitMQConstants.FrameMethod, _channelId, 13, ref writer);
            FrameWriter.WriteMethodFrame(60, 80, ref writer);
            writer.WriteLongLong(message.DeliveryTag);
            writer.WriteBit(message.Multiple);
            writer.BitFlush();
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
            writer.Commit();
        }
    }
}
