using AMQP.Client.RabbitMQ.Protocol.Framing;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Common
{
    /* 
     *                      Method Frame
     * 
     * 0          2          4
     * +----------+----------+---------------------------------+
     * | short    | short    |            MethodData           |
     * +----------+----------+---------------------------------+
     *   class-id   method-id
     */
    public class MethodHeaderReader : IMessageReader<MethodHeader>, IMessageReaderAdapter<MethodHeader>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out MethodHeader message)
        {
            message = default;
            SequenceReader<byte> reader = new SequenceReader<byte>(input.Slice(consumed));
            if (!reader.TryReadBigEndian(out short classId)) { return false; }
            if (!reader.TryReadBigEndian(out short methodId)) { return false; }
            message = new MethodHeader(classId, methodId);
            consumed = reader.Position;
            examined = consumed;
            return true;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, out MethodHeader message)
        {
            message = default;
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            if (!reader.TryReadBigEndian(out short classId)) { return false; }
            if (!reader.TryReadBigEndian(out short methodId)) { return false; }
            message = new MethodHeader(classId, methodId);
            return true;
        }
    }
}
