using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class HeartbeatWriter : IMessageWriter<bool>
    {
        private static readonly ReadOnlyMemory<byte> _heartbeatFrame = new byte[8] { 8, 0, 0, 0, 0, 0, 0, 206 };
        public void WriteMessage(bool _, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.Write(_heartbeatFrame.Span);
            writer.Commit();
        }
    }
}
