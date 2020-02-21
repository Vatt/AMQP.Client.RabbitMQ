using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System.Buffers;
using System;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public class BasicPublishWriter : IMessageWriter<BasicPublishInfo>
    {
        private readonly ushort _channelid;
        public BasicPublishWriter(ushort channelId)
        {
            _channelid = channelId;
        }
        public void WriteMessage(BasicPublishInfo message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            var payloadSize = 4 + 2 + message.ExchangeName.Length + 1 + message.RoutingKey.Length + 1 + 1;
            FrameWriter.WriteFrameHeader(Constants.FrameMethod, _channelid, payloadSize, ref writer);
            FrameWriter.WriteMethodFrame(60, 40, ref writer);
            writer.WriteShortInt(0); //reserved-1
            writer.WriteShortStr(message.ExchangeName);
            writer.WriteShortStr(message.RoutingKey);
            writer.WriteBit(message.Mandatory);
            writer.WriteBit(message.Immediate);
        }
    }
}
