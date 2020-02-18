using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public class BasicDeliverReader : IMessageReader<DeliverInfo>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out DeliverInfo message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            //if(!reader.ReadShortStr(out var consumerTag)) { return false; }
            //if(!reader.ReadLongLong(out var deliveryTag)) { return false; }
            //if(!reader.ReadBool(out bool redelivered)) { return false; }
            //if(!reader.ReadShortStr(out var exchangeName)) { return false; }
            //if(!reader.ReadShortStr(out var routingKey)) { return false; }
            //if(!reader.ReadOctet(out byte endMarker)) { return false; }
            reader.ReadShortStr(out var consumerTag);
            reader.ReadLongLong(out var deliveryTag);
            reader.ReadBool(out bool redelivered);
            reader.ReadShortStr(out var exchangeName);
            reader.ReadShortStr(out var routingKey);
            reader.ReadOctet(out byte endMarker);
            if(endMarker != Constants.FrameEnd)
            {                
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            message = new DeliverInfo(consumerTag, deliveryTag, redelivered, exchangeName, routingKey);
            consumed = reader.Position;
            examined = consumed;
            return true;
        }
    }
}
