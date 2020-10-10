using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    internal class CloseReader : IMessageReader<CloseInfo>, IMessageReaderAdapter<CloseInfo>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out CloseInfo message)
        {
            message = default;
            ValueReader reader = new ValueReader(input, consumed);
            if (!reader.ReadShortInt(out short replyCode)) { return false; }
            if (!reader.ReadShortStr(out var replyText)) { return false; }
            if (!reader.ReadShortInt(out short failedClassId)) { return false; }
            if (!reader.ReadShortInt(out short failedMethodId)) { return false; }
            if (!reader.ReadOctet(out var endMarker)) { return false; }
            if (endMarker != RabbitMQConstants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            message = CloseInfo.Create(replyCode, replyText, failedClassId, failedMethodId);

            consumed = reader.Position;
            examined = consumed;
            return true;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out CloseInfo message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadShortInt(out short replyCode)) { return false; }
            if (!reader.ReadShortStr(out var replyText)) { return false; }
            if (!reader.ReadShortInt(out short failedClassId)) { return false; }
            if (!reader.ReadShortInt(out short failedMethodId)) { return false; }
            message = CloseInfo.Create(replyCode, replyText, failedClassId, failedMethodId);;
            return true;
        }
    }
}
