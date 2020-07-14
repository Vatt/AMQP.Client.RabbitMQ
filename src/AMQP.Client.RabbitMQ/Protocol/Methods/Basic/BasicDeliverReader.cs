using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    internal class BasicDeliverReader : IMessageReader<RabbitMQDeliver>, IMessageReaderAdapter<RabbitMQDeliver>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out RabbitMQDeliver message)
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
            message = new RabbitMQDeliver(consumerTag, exchangeName, routingKey, deliveryTag, redelivered);
            consumed = reader.Position;
            examined = consumed;
            return true;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out RabbitMQDeliver message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadShortStr(out var consumerTag)) { return false; }
            if (!reader.ReadLongLong(out var deliveryTag)) { return false; }
            if (!reader.ReadBool(out bool redelivered)) { return false; }
            if (!reader.ReadShortStr(out var exchangeName)) { return false; }
            if (!reader.ReadShortStr(out var routingKey)) { return false; }
            message = new RabbitMQDeliver(consumerTag, exchangeName, routingKey, deliveryTag, redelivered);
            return true;
        }
    }
}
