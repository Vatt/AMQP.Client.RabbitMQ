using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    internal class BasicRejectWriter : IMessageWriter<RejectInfo>
    {
        public void WriteMessage(RejectInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(RabbitMQConstants.FrameMethod, message.ChannelId, 13, ref writer);
            FrameWriter.WriteMethodFrame(60, 90, ref writer);
            writer.WriteLongLong(message.DeliveryTag);
            writer.WriteBit(message.Requeue);
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
        }
    }
}
