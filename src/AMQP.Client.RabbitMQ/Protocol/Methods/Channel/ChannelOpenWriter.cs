using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Channel
{
    class ChannelOpenWriter : IMessageWriter<ushort>
    {
        public void WriteMessage(ushort message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(1, message, 5, ref writer);
            FrameWriter.WriteMethodFrame(20, 10, ref writer);
            writer.WriteOctet(0);
            writer.WriteOctet(206);
            writer.Commit();
        }
    }
}
