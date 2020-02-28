using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public class BasicQoSWriter : IMessageWriter<QoSInfo>
    {
        private readonly ushort _channel;
        public BasicQoSWriter(ushort channel)
        {
            _channel = channel;
        }
        public void WriteMessage(QoSInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(1, _channel, 11, ref writer);
            FrameWriter.WriteMethodFrame(60, 10, ref writer);
            writer.WriteLong(message.PrefetchSize);
            writer.WriteShortInt(message.PrefetchCount);
            writer.WriteBool(message.Global);
            writer.WriteOctet(Constants.FrameEnd);

            writer.Commit();
        }
    }
}
