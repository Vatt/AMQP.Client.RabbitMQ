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
        private ContentHeader _header;
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ReadOnlySequence<byte> message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);

            var readable = Math.Min((_header.BodySize - Consumed), input.Length);
            message = input.Slice(0,readable);
            Consumed += readable;
            reader.Advance(readable);
            if (Consumed == _header.BodySize)
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
        public void Restart(ContentHeader header)
        {
            Consumed = 0;
            _header = header;
        }
    }
}
