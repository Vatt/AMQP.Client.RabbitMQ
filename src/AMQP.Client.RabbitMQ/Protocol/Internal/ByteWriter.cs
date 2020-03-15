using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Internal
{
    internal class ByteWriter : IMessageWriter<byte[]>
    {
        public void WriteMessage(byte[] message, IBufferWriter<byte> output)
        {
            if (message.Length > 1024)
            {
                throw new Exception($"{nameof(ByteWriter)}:message to long. Maximum length - 1024");
            }
            output.Write(message);
        }
    }
}
