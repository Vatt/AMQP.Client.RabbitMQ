using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class PublishInfoAndContentWriter : IMessageWriter<PublishInfoAndContent>
    {
        private ushort _channelId;
        private BasicPublishWriter _basicPublishWriter;
        private ContentHeaderWriter _contentHeaderWriter;

        public PublishInfoAndContentWriter(ushort channelId)
        {
            _channelId = channelId;
            _basicPublishWriter = new BasicPublishWriter(_channelId);
            _contentHeaderWriter = new ContentHeaderWriter(_channelId);
        }

        public void WriteMessage(PublishInfoAndContent message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);
            _basicPublishWriter.WriteMessage(ref message.Info, ref writer);
            _contentHeaderWriter.WriteMessage(ref message.Header, ref writer);
            writer.Commit();
        }
    }

    internal class PublishInfoAndContent
    {
        private BasicPublishInfo _info;
        private ContentHeader _contentHeader;

        public PublishInfoAndContent(ref BasicPublishInfo info, ref ContentHeader header)
        {
            _info = info;
            _contentHeader = header;
        }

        public ref BasicPublishInfo Info => ref _info;
        public ref ContentHeader Header => ref _contentHeader;
    }
}
