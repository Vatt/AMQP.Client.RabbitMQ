﻿using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class PublishInfoAndContentWriter : IMessageWriter<PublishPartialInfo>
    {
        private ushort _channelId;
        private BasicPublishWriter _basicPublishWriter;

        public PublishInfoAndContentWriter(ushort channelId)
        {
            _channelId = channelId;
            _basicPublishWriter = new BasicPublishWriter();
        }

        public void WriteMessage(PublishPartialInfo message, IBufferWriter<byte> output)
        {
            var writer = new ValueWriter(output);
            _basicPublishWriter.WriteMessage(ref message.Info, ref writer);
            ProtocolWriters.ContentHeaderWriter.WriteMessage(ref message.Header, ref writer);
            writer.Commit();
        }
    }

    public class PublishPartialInfo
    {
        private BasicPublishInfo _info;
        private ContentHeader _contentHeader;

        public PublishPartialInfo(ref BasicPublishInfo info, ref ContentHeader header)
        {
            _info = info;
            _contentHeader = header;
        }

        public ref BasicPublishInfo Info => ref _info;
        public ref ContentHeader Header => ref _contentHeader;
    }
}
