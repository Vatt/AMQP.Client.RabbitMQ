using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    internal class BasicRejectWriter : IMessageWriter<RejectInfo>
    {
        private readonly ushort _channel;
        public BasicRejectWriter(ushort channel)
        {
            _channel = channel;
        }
        public void WriteMessage(RejectInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(Constants.FrameMethod, _channel, 13, ref writer);
            FrameWriter.WriteMethodFrame(60, 90, ref writer);
            writer.WriteLongLong(message.DeliveryTag);
            writer.WriteBit(message.Requeue);
            writer.WriteOctet(Constants.FrameEnd);
        }
    }
}
