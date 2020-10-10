using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class BodyFrameWriter : IMessageWriter<(ushort, ReadOnlyMemory<byte>)>
    {
        public void WriteMessage((ushort, ReadOnlyMemory<byte>) message, IBufferWriter<byte> output)
        {
            if (message.Item2.IsEmpty) { return; }
            var writer = new ValueWriter(output);
            FrameWriter.WriteFrameHeader(RabbitMQConstants.FrameBody, message.Item1, message.Item2.Length, ref writer);
            writer.WriteBytes(message.Item2.Span);
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
            writer.Commit();
        }

        internal void WriteMessage((ushort, ReadOnlyMemory<byte>) message, ref ValueWriter writer)
        {
            FrameWriter.WriteFrameHeader(RabbitMQConstants.FrameBody, message.Item1, message.Item2.Length, ref writer);
            writer.WriteBytes(message.Item2.Span);
            writer.WriteOctet(RabbitMQConstants.FrameEnd);
            writer.Commit();
        }
    }
}
