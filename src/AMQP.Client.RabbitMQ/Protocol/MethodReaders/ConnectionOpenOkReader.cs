using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.MethodReaders
{
    public class ConnectionOpenOkReader : IMessageReader<bool>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out bool message)
        {
            message = false;
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if(reader.Remaining < 8)
            {
                return false;
            }
            reader.Advance(8);
            message = true;
            consumed = reader.Position;
            examined = consumed;
            return true;
        }
    }
}
