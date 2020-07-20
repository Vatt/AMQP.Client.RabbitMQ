using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class ShortStrPayloadReader : IMessageReader<string>, IMessageReaderAdapter<string>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out string message)
        {
            var reader = new ValueReader(input, consumed);
            if (!reader.ReadShortStr(out message)) { return false; }

            if (!reader.ReadOctet(out var endMarker)) { return false; }

            if (endMarker != RabbitMQConstants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }

            consumed = reader.Position;
            examined = consumed;
            return true;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out string message)
        {
            var reader = new ValueReader(input);
            if (!reader.ReadShortStr(out message)) { return false; }
            return true;
        }
    }
}
