using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public class ConnectionStartReader : IMessageReader<RabbitMQServerInfo>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out RabbitMQServerInfo message)
        {
            
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadOctet(out var major)) { return false; }
            if (!reader.ReadOctet(out var minor)) { return false; }
            if (!reader.ReadTable(out var tab)) { return false; }
            if (!reader.ReadLongStr(out var mechanisms)) { return false; }
            if (!reader.ReadLongStr(out var locales)) { return false; }
            if (!reader.ReadOctet(out var end_frame_marker)) { return false; }
            if (end_frame_marker != 206)
            {
                ReaderThrowHelper.ThrowIfEndMarkerMissmatch();
            }
            message = new RabbitMQServerInfo(major, minor, tab, mechanisms, locales);
            consumed = reader.Position;
            examined = reader.Position;
            return true;
        }
    }
}
