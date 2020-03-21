using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    /* 
     *               Tune method frame
     * 
     * 0          2          4           6           10          12
     * +----------+----------+-----------+-----------+-----------+
     * | short    | short    |channel-max| frame-max | heartbeat |
     * +----------+----------+-----------+-----------+-----------+
     *      10        31          short       int        short
     */
    internal class ConnectionTuneOkWriter : IMessageWriter<RabbitMQMainInfo>
    {
        public void WriteMessage(RabbitMQMainInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(1, 0, 12, ref writer);
            FrameWriter.WriteMethodFrame(10, 31, ref writer);
            writer.WriteShortInt(message.ChannelMax);
            writer.WriteLong(message.FrameMax);
            writer.WriteShortInt(message.Heartbeat);
            writer.WriteOctet(206);
            writer.Commit();
        }
    }
}
