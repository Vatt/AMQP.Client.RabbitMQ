using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public class QueuePurgeOkDeleteOkReader : IMessageReader<int>
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
