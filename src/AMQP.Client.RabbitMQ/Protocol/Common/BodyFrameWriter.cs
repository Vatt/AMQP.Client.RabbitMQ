using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Infrastructure;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public class BodyFrameWriter : IMessageWriter<ReadOnlyMemory<byte>>
    {
        private readonly ushort _channelId;
        public BodyFrameWriter(ushort channelId)
        {
            _channelId = channelId;
        }
        public void WriteMessage(ReadOnlyMemory<byte> message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(Constants.FrameBody, _channelId, message.Length, ref writer);
            writer.WriteBytes(message.Span);
            writer.WriteOctet(Constants.FrameEnd);
            writer.Commit();
        }
        internal void WriteMessage(ref ReadOnlyMemory<byte> message, ref ValueWriter writer)
        {
            FrameWriter.WriteFrameHeader(Constants.FrameBody, _channelId, message.Length, ref writer);
            writer.WriteBytes(message.Span);
            writer.WriteOctet(Constants.FrameEnd);
            writer.Commit();
        }
    }
}
