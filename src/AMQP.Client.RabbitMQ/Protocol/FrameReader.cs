using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;

namespace AMQP.Client.RabbitMQ.Protocol
{
    internal class FrameReader : IMessageReader<ReadOnlySequence<byte>>, IMessageReaderAdapter<Frame>
    {
        private static readonly FrameHeaderReader frameReader = new FrameHeaderReader();
        private int _consumed;
        public int FrameSize { get; private set; }
        public bool IsComplete { get; private set; }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed,
            ref SequencePosition examined, out ReadOnlySequence<byte> message)
        {
            message = default;
            if (input.IsEmpty) return false;
            var reader = new ValueReader(input);
            if (_consumed == 0)
            {
                if (!ReadHeader(out var type, out var channel, out var payloadSize, ref reader)) return false;
                FrameSize = 7 + payloadSize + 1; // header + payload + endMarker
                if (input.Length >= FrameSize)
                {
                    message = input.Slice(0, FrameSize);
                    reader.Advance(FrameSize - reader.Consumed);
                    consumed = reader.Position;
                    examined = consumed;
                    IsComplete = true;
                    return true;
                }
            }


            var readable = Math.Min(FrameSize - _consumed, input.Length);
            message = input.Slice(0, readable);
            _consumed += (int)readable;
            reader.Advance(readable - reader.Consumed);
            if (_consumed == FrameSize)
            {
                //if (!reader.ReadOctet(out byte marker) || marker != Constants.FrameEnd)
                //{
                //    ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
                //}
                consumed = reader.Position;
                examined = consumed;
                IsComplete = true;
                return true;
            }

            consumed = reader.Position;
            examined = consumed;
            return true;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out Frame message)
        {
            message = default;

            var try_read = frameReader.TryParseMessage(input, out var header);
            if (!try_read) return false;

            var reader = new SequenceReader<byte>(input.Slice(7));
            message = new Frame(header, input.Slice(reader.Position, header.PayloadSize));
            reader.Advance(message.Payload.Length);
            if (!reader.TryRead(out var endMarker)) ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            if (endMarker != RabbitMQConstants.FrameEnd) ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            return true;
        }

        public void Reset()
        {
            _consumed = 0;
            FrameSize = 0;
            IsComplete = false;
        }

        private bool ReadHeader(out byte type, out ushort channel, out int payloadSize, ref ValueReader reader)
        {
            channel = default;
            payloadSize = default;
            if (!reader.ReadOctet(out type)) return false;
            if (!reader.ReadShortInt(out channel)) return false;
            if (!reader.ReadLong(out payloadSize)) return false;
            return true;
        }
    }
}