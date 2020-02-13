using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods
{
    public class HeartbeatReader : IMessageReader<bool>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out bool message)
        {
            var reader = new SequenceReader<byte>(input);
            if(!reader.TryRead(out byte endMarker))
            {
                message = false;
                return false;
            }
            Debug.Assert(endMarker == 206);
            if (endMarker != 206)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            message = true;
            consumed = reader.Position;
            examined = consumed;
            return true;

        }
    }
}
