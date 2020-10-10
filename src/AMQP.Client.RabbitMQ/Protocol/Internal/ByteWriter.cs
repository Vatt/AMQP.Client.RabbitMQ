using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Exceptions;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Internal
{
    internal class ByteWriter : IMessageWriter<ReadOnlyMemory<byte>>
    {
        public void WriteMessage(ReadOnlyMemory<byte> message, IBufferWriter<byte> output)
        {
            if (message.Length > 1024)
            {
                throw new RabbitMQException($"{nameof(ByteWriter)}:message to long. Maximum length - 1024");
            }
            output.Write(message.Span);
        }
    }
}
