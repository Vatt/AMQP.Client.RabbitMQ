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
        public long Consumed { get; private set; } = 0;
        private FrameHeader _header;
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ReadOnlySequence<byte> message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);

            var readable = Math.Min((_header.PaylodaSize - Consumed), input.Length);
            message = input.Slice(reader.Position, readable);
            Consumed += readable;
            reader.Advance(readable);
            if (Consumed == _header.PaylodaSize)
            {
                
                reader.ReadOctet(out var endMarker);
                if (endMarker != Constants.FrameEnd)
                {
                    ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
                }
            }

            
            consumed = reader.Position;
            examined = consumed;
            return true;
            
        }
        public void Restart(FrameHeader header)
        {
            Consumed = 0;
            _header = header;
        }
    }
}
