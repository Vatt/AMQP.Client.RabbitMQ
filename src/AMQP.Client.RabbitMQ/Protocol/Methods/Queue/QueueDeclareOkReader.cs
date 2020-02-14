using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public class QueueDeclareOkReader : IMessageReader<QueueDeclareOk>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out QueueDeclareOk message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if(!reader.ReadShortStr(out var Name)) { return false; }
            if(!reader.ReadLong(out var messageCount)) { return false; }
            if(!reader.ReadLong(out var consumerCount)) { return false; }
            if(!reader.ReadOctet(out var endMarker)) { return false; }
            if (endMarker != Constants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            consumed = reader.Position;
            examined = consumed;
            message = new QueueDeclareOk(Name, messageCount, consumerCount);
            return true;
        }
    }
}
