using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.MethodWriters
{
    public class ByteWriter : IMessageWriter<byte[]>
    {
        public void WriteMessage(byte[] message, IBufferWriter<byte> output)
        {
            output.Write(message);
        }
    }
}
