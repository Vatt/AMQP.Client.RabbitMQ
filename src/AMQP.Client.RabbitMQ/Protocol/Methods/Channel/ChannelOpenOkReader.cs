using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Channel
{
    public class ChannelOpenOkReader : IMessageReader<bool>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out bool message)
        {
            message = false;
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if (reader.Remaining < 5)
            {
                return false;
            }
            reader.Advance(4);
            var result = reader.TryRead(out byte end);
            if (end != 206 || result == false)
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
