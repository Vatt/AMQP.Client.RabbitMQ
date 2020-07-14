using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public class NoPayloadReader : IMessageReader<bool>, IMessageReaderAdapter<bool>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out bool message)
        {
            message = false;
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if (!reader.TryRead(out var endMarker))
            {
                return false;
            }
            if (endMarker != Constants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            consumed = reader.Position;
            examined = consumed;
            message = true;
            return true;

        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out bool message)
        {
            message = input.Length == 0;
            return message;
        }
    }
}
