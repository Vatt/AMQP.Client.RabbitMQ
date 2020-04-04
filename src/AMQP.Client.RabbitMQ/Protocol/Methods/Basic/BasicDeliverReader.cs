using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    internal class BasicDeliverReader : IMessageReader<Deliver>, IMessageReaderAdapter<Deliver>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out Deliver message)
        {
            message = default;
            ValueReader reader = new ValueReader(input, consumed);
            if (!reader.ReadShortStr(out var consumerTag)) { return false; }
            if (!reader.ReadLongLong(out var deliveryTag)) { return false; }
            if (!reader.ReadBool(out bool redelivered)) { return false; }
            if (!reader.ReadShortStr(out var exchangeName)) { return false; }
            if (!reader.ReadShortStr(out var routingKey)) { return false; }
            if (!reader.ReadOctet(out byte endMarker)) { return false; }
            if (endMarker != Constants.FrameEnd)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            message = new Deliver(consumerTag, deliveryTag, redelivered, exchangeName, routingKey);
            consumed = reader.Position;
            examined = consumed;
            return true;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out Deliver message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadShortStr(out var consumerTag)) { return false; }
            if (!reader.ReadLongLong(out var deliveryTag)) { return false; }
            if (!reader.ReadBool(out bool redelivered)) { return false; }
            if (!reader.ReadShortStr(out var exchangeName)) { return false; }
            if (!reader.ReadShortStr(out var routingKey)) { return false; }
            message = new Deliver(consumerTag, deliveryTag, redelivered, exchangeName, routingKey);
            return true;
        }
    }
}
