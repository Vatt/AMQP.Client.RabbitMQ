using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class PublishFullWriter : IMessageWriter<PublishFullContent>
    {
        private ushort _channelId;
        private BasicPublishWriter _basicPublishWriter;
        private ContentHeaderWriter _contentHeaderWriter;
        private BodyFrameWriter _bodyFrameWriter;

        public PublishFullWriter(ushort channelId)
        {
            _channelId = channelId;
            _basicPublishWriter = new BasicPublishWriter(_channelId);
            _contentHeaderWriter = new ContentHeaderWriter(_channelId);
            _bodyFrameWriter = new BodyFrameWriter(_channelId);
        }

        public void WriteMessage(PublishFullContent message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);
            _basicPublishWriter.WriteMessage(ref message.Info, ref writer);
            _contentHeaderWriter.WriteMessage(ref message.Header, ref writer);
            _bodyFrameWriter.WriteMessage(message.Body, ref writer);
            writer.Commit();
        }
    }

    internal class PublishFullContent
    {
        private BasicPublishInfo _info;
        private ContentHeader _contentHeader;
        public ReadOnlyMemory<byte> Body { get; }

        public PublishFullContent(ReadOnlyMemory<byte> body, ref BasicPublishInfo info, ref ContentHeader header)
        {
            Body = body;
            _info = info;
            _contentHeader = header;
        }

        public ref BasicPublishInfo Info => ref _info;
        public ref ContentHeader Header => ref _contentHeader;
    }
}
