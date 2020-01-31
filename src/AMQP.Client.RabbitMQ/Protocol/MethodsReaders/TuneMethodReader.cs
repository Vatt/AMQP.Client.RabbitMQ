using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.MethodReaders
{
    public class TuneMethodReader:IMessageReader<RabbitMQInfo>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out RabbitMQInfo message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadShortInt(out var chanellMax)) { return false; }
            if (!reader.ReadLong(out var frameMax)) { return false; }
            if (!reader.ReadShortInt(out var heartbeat)) { return false; }
            if (!reader.ReadOctet(out var end_frame_marker)) { return false; }
            if (end_frame_marker != 206)
            {
                ValueReaderThrowHelper.ThrowIfFrameDecoderEndMarkerMissmatch();
            }
            message = new RabbitMQInfo(chanellMax, frameMax, heartbeat);
            consumed = reader.Position;
            examined = reader.Position;
            return true;
        }
    }
}