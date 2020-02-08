using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.MethodWriters
{
    public class ConnectionTuneOkWriter :IMessageWriter<RabbitMQMainInfo>
    {
        public void WriteMessage(RabbitMQMainInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(1, 0, 12,ref writer);
            FrameWriter.WriteMethodFrame(10, 31, ref writer);
            writer.WriteShortInt(10);
            writer.WriteShortInt(11);
            writer.WriteShortInt(message.ChanellMax);
            writer.WriteLong(message.FrameMax);
            writer.WriteShortInt(message.Heartbeat);
            writer.WriteOctet(206);
            writer.Commit();
        }
    }
}
