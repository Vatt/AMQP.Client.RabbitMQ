using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    public class BodyFrameChunkedReader : IMessageReader<ReadOnlySequence<byte>>
    {
        private long _consumed = 0;
        private ContentHeader _header;
        public bool IsComplete { get; private set; }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ReadOnlySequence<byte> message)
        {
            message = default;
            if (input.Length == 0) { return false; }
            ValueReader reader = new ValueReader(input);

            if (_consumed == _header.BodySize)
            {
                // if (!TryReadEndMarker(ref reader)) { return false; }
                if (!reader.ReadOctet(out byte marker)) { return false; }
                if (marker != Constants.FrameEnd)
                {
                    ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
                }
                consumed = reader.Position;
                examined = consumed;
                IsComplete = true;
                return true;
            }

            var readable = Math.Min((_header.BodySize - _consumed), input.Length);
            message = input.Slice(reader.Position,readable);
            _consumed += readable;
            reader.Advance(readable);
            if (_consumed == _header.BodySize)
            {
                //if (!TryReadEndMarker(ref reader)) { return false; }
                if (!reader.ReadOctet(out byte marker)) 
                {
                    _consumed -= readable;
                    return false; 
                }
                if (marker != Constants.FrameEnd)
                {
                    ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
                }
                consumed = reader.Position;
                examined = consumed;
                IsComplete = true;
                return true;
            }

            IsComplete = false;
            consumed = reader.Position;
            examined = consumed;
            return true;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryReadEndMarker(ref ValueReader reader)
        {
            if (!reader.ReadOctet(out byte marker)) { return false; }
            if (marker != Constants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            return true;
        }
        public void Restart(ContentHeader header)
        {
            _consumed = 0;
            IsComplete = false;
            _header = header;
        }
    }
}
