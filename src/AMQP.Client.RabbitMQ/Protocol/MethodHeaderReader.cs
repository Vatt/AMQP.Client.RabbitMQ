using AMQP.Client.RabbitMQ.Protocol.Framing;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol
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
    public class MethodHeaderReader : IMessageReader<MethodHeader>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out MethodHeader message)
        {
            //class-id method-id:short short
            if (input.Length < 4)
            {
                message = default;
                return false;
            }
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            reader.TryReadBigEndian(out short classId);
            reader.TryReadBigEndian(out short methodId);
            message = new MethodHeader(classId, methodId);
            consumed = reader.Position;
            examined = consumed;
            return true;
        }
    }
}
