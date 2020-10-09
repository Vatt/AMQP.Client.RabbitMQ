using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class BodyFrameWriter : IMessageWriter<ReadOnlyMemory<byte>>
    {
        private readonly ushort _channelId;
        public BodyFrameWriter(ushort channelId)
        {
            _channelId = channelId;
        }

        public void WriteMessage(ReadOnlyMemory<byte> message, IBufferWriter<byte> output)
        {
            if (message.IsEmpty) { return; }
            var writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(RabbitMQConstants.FrameBody, _channelId, message.Length, ref writer);
            writer.WriteBytes(message.Span);
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
            writer.Commit();
        }

        internal void WriteMessage(ReadOnlyMemory<byte> message, ref ValueWriter writer)
        {
            FrameWriter.WriteFrameHeader(RabbitMQConstants.FrameBody, _channelId, message.Length, ref writer);
            writer.WriteBytes(message.Span);
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
            writer.Commit();
        }
    }
}
