using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    internal class ConnectionOpenOkReader : IMessageReader<bool>, IMessageReaderAdapter<bool>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out bool message)
        {
            message = false;
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if (reader.Remaining < 2) //reserved­1 + end_marker
            {
                return false;
            }
            reader.Advance(2);
            //reader.Advance(1);
            //if (!reader.TryRead(out var endMarker))
            //{
            //    return false;
            //}
            //if (endMarker != Constants.FrameEnd)
            //{
            //    ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            //}
            message = true;
            consumed = reader.Position;
            examined = consumed;
            return true;
        }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, out bool message)
        {
            message = false;
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if (reader.Remaining < 1) //reserved­1
            {
                return false;
            }
            reader.Advance(1);
            message = true;
            return message;
        }
    }
}
