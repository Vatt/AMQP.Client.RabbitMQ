using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods
{
    public class CloseOkReader : IMessageReader<bool>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out bool message)
        {
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            reader.TryRead(out byte endMarker);
            if(endMarker != 206)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            consumed = reader.Position;
            examined = consumed;
            message = true;
            return true;
        }
    }
}
