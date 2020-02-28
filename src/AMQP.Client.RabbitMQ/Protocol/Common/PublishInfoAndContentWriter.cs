using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Bedrock.Framework.Protocols;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public class PublishInfoAndContentWriter : IMessageWriter<(BasicPublishInfo, ContentHeader)>
    {
        private readonly ushort _channelId;
        public PublishInfoAndContentWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage((BasicPublishInfo, ContentHeader) message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            var publishWriter = new BasicPublishWriter(_channelId);
            var contentWriter = new ContentHeaderWriter(_channelId);
            publishWriter.WriteMessage(ref message.Item1, ref writer);
            contentWriter.WriteMessage(ref message.Item2, ref writer);
            writer.Commit();
        }
    }
}
