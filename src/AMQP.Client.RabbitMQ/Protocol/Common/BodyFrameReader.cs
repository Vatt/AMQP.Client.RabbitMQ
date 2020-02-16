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
    public class BodyFrameReader : IMessageReader<byte[]>
    {
        private int Consumed = 0;
        private FrameHeader _header;
        private Memory<byte> _buffer;

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out byte[] message)
        {
            message = default;
            
            SequenceReader<byte> reader = new SequenceReader<byte>(input);

            int readable = (int) Math.Min((_header.PaylodaSize - Consumed), reader.Length);
            var span = _buffer.Slice(Consumed, readable).Span;
            input.Slice(0, readable).CopyTo(span);
            Consumed += readable;
            reader.Advance(readable);
            if (Consumed == _header.PaylodaSize)
            {

                reader.TryRead(out var endMarker);
                if (endMarker != Constants.FrameEnd)
                {
                    ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
                }
            }

            
            consumed = reader.Position;
            examined = consumed;
            return true;
            
        }
        public void Reset(FrameHeader header, Memory<byte> buffer)
        {
            _header = header;
            _buffer = buffer;
            Consumed = 0;
        }

    }
}
