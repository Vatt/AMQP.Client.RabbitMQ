using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public class PublishFullWriter : IMessageWriter<(BasicPublishInfo, ContentHeader, ReadOnlyMemory<byte>)>
    {
        private readonly ushort _channelId;
        public PublishFullWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage((BasicPublishInfo, ContentHeader, ReadOnlyMemory<byte>) message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            var publishWriter = new BasicPublishWriter(_channelId);
            var contentWriter = new ContentHeaderWriter(_channelId);
            var bodyFrameWriter = new BodyFrameWriter(_channelId);
            publishWriter.WriteMessage(ref message.Item1, ref writer);
            contentWriter.WriteMessage(ref message.Item2, ref writer);
            bodyFrameWriter.WriteMessage(ref message.Item3, ref writer);
            writer.Commit();
        }
    }

}
