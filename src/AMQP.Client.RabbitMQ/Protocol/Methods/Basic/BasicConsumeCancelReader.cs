using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    class BasicConsumeCancelReader : IMessageReader<ConsumeCancelInfo>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ConsumeCancelInfo message)
        {
            message = default;
            var reader = new ValueReader(input, consumed);
            if (!reader.ReadShortStr(out string tag)) { return false; }
            if (!reader.ReadBool(out bool noWait)) { return false; }
            if (!reader.ReadOctet(out var endMarker)) { return false; }

            if (endMarker != RabbitMQConstants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }

            consumed = reader.Position;
            examined = consumed;
            message = new ConsumeCancelInfo(tag, noWait);
            return true;
        }
    }
}
