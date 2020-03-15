using System;
using System.Buffers;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public class ConnectionOpenOkReader : IMessageReader<bool>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out bool message)
        {
            message = false;
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if (reader.Remaining < 2)
            {
                return false;
            }
            reader.Advance(2);
            message = true;
            consumed = reader.Position;
            examined = consumed;
            return true;
        }
    }
}
