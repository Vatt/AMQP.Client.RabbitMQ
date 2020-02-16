using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public class ShortStrPayloadReader : IMessageReader<string>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out string message)
        {
            ValueReader reader = new ValueReader(input);
            if(!reader.ReadShortStr(out message)) { return false; }

            if(!reader.ReadOctet(out var endMarker)) { return false; }

            if(endMarker != Constants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }

            consumed = reader.Position;
            examined = consumed;
            return true;
        }
    }
}
