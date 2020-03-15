using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Common
{
    public class CloseReader : IMessageReader<CloseInfo>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out CloseInfo message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadShortInt(out short replyCode)) { return false; }
            if (!reader.ReadShortStr(out var replyText)) { return false; }
            if (!reader.ReadShortInt(out short failedClassId)) { return false; }
            if (!reader.ReadShortInt(out short failedMethodId)) { return false; }
            if (!reader.ReadOctet(out var endMarker)) { return false; }
            if (endMarker != Constants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            message = new CloseInfo(replyCode, replyText, failedClassId, failedMethodId);

            consumed = reader.Position;
            examined = consumed;
            return true;
        }
    }
}
