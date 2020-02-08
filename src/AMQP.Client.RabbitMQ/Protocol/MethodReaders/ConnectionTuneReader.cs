using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.MethodReaders
{
    public class ConnectionTuneReader:IMessageReader<RabbitMQMainInfo>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out RabbitMQMainInfo message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadShortInt(out var chanellMax)) { return false; }
            if (!reader.ReadLong(out var frameMax)) { return false; }
            if (!reader.ReadShortInt(out var heartbeat)) { return false; }
            if (!reader.ReadOctet(out var end_frame_marker)) { return false; }
            if (end_frame_marker != 206)
            {
                ReaderThrowHelper.ThrowIfFrameDecoderEndMarkerMissmatch();
            }
            message = new RabbitMQMainInfo(chanellMax, frameMax, heartbeat);
            consumed = reader.Position;
            examined = reader.Position;
            return true;
        }
    }
}