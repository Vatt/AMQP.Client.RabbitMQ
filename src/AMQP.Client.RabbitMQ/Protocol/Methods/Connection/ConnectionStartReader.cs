using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    internal class ConnectionStartReader : IMessageReader<ServerConf>, IMessageReaderAdapter<ServerConf>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ServerConf message)
        {

            message = default;
            ValueReader reader = new ValueReader(input, consumed);
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
            message = new ServerConf(major, minor, tab, mechanisms, locales);
            consumed = reader.Position;
            examined = reader.Position;
            return true;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out ServerConf message)
        {
            message = default;
            ValueReader reader = new ValueReader(input);
            if (!reader.ReadOctet(out var major)) { return false; }
            if (!reader.ReadOctet(out var minor)) { return false; }
            if (!reader.ReadTable(out var tab)) { return false; }
            if (!reader.ReadLongStr(out var mechanisms)) { return false; }
            if (!reader.ReadLongStr(out var locales)) { return false; }
            message = new ServerConf(major, minor, tab, mechanisms, locales);
            return true;
        }
    }
}
