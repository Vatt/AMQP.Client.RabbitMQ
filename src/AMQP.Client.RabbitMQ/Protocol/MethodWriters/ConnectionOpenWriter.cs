using AMQP.Client.RabbitMQ.Protocol.Internal;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Buffers.Binary;

namespace AMQP.Client.RabbitMQ.Protocol.MethodWriters
{
    public class ConnectionOpenWriter : IMessageWriter<string>
    {
        public void WriteMessage(string message, IBufferWriter<byte> output)
        {
            ValueWriter writer = new ValueWriter(output);
            writer.WriteOctet(1);
            writer.WriteShortInt(0);
            var reserved = writer.Reserve(4);
            var checkpoint = writer.Written;
            FrameWriter.WriteMethodFrame(10, 40, ref writer);
            writer.WriteShortStr(message);
            writer.WriteOctet(0);
            writer.WriteOctet(0);
            var paylodaSize = writer.Written - checkpoint;
            writer.WriteOctet(206);
            Span<byte> bytes = stackalloc byte[4];
            BinaryPrimitives.WriteInt32BigEndian(bytes, paylodaSize);
            reserved.Write(bytes);
            writer.Commit();
        }
    }
}
