using System;
using System.Buffers;
using System.IO.Pipelines;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using Bedrock.Framework.Protocols;
namespace AMQP.Client.RabbitMQ.Protocol
{
    public class FrameHeaderReader : IMessageReader<FrameHeader>
    {
        public FrameHeaderReader()
        {
        }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out FrameHeader message)
        {
            SequenceReader<byte> reader = new SequenceReader<byte>();
            message = default;
            //Frame header = byte + short + int
            if (reader.Remaining < 7)
            {
                return false;
            }
            reader.TryRead(out byte type);
            reader.TryReadBigEndian(out short chanell);
            reader.TryReadBigEndian(out int payloadSize);
            message = new FrameHeader(type, chanell, payloadSize);

            consumed = reader.Position;
            examined = reader.Position;
            return true;

        }
    }

}
