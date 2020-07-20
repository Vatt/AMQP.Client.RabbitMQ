using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    internal class QueuePurgeOkDeleteOkReader : IMessageReader<int>, IMessageReaderAdapter<int>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out int message)
        {
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if (!reader.TryReadBigEndian(out message))
            {
                return false;
            }
            if (!reader.TryRead(out byte endMarker))
            {
                return false;
            }
            if (endMarker != RabbitMQConstants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            consumed = reader.Position;
            examined = consumed;
            return true;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out int message)
        {
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if (!reader.TryReadBigEndian(out message))
            {
                return false;
            }
            return true;
        }
    }
}
