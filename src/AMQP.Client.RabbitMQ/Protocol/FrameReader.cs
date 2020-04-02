using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol
{
    internal class FrameReader : IMessageReader<Frame>
    {
        private FrameHeaderReader frameReader = new FrameHeaderReader();
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out Frame message)
        {
            message = default;

            var try_read = frameReader.TryParseMessage(input, ref consumed, ref examined, out var header);
            if (!try_read)
            {
                return false;
            }
            if (input.Length < header.PaylodaSize)
            {
                return false;
            }
            SequenceReader<byte> reader = new SequenceReader<byte>(input.Slice(consumed));
            message = new Frame(header, input.Slice(consumed, header.PaylodaSize));
            reader.Advance(message.Payload.Length);
            if (!reader.TryRead(out byte endMarker))
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            if (endMarker != Constants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            consumed = reader.Position;
            examined = consumed;
            return true;
        }
    }
}
