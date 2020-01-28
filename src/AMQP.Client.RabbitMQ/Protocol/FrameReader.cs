using System;
using System.Buffers;
using System.IO.Pipelines;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using Bedrock.Framework.Protocols;
namespace AMQP.Client.RabbitMQ.Protocol
{
    public class FrameReader : IMessageReader<ReadOnlySequence<byte>>
    {
        public FrameReader()
        {
        }
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ReadOnlySequence<byte> message)
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
            Frame frame = new Frame(type,chanell,payloadSize);
            switch(frame.FrameType)
            {
                case 1:
                    {
                        consumed = reader.Position;
                        if (TryParseMethodFrame(input, ref consumed, ref examined, out MethodFrame method))
                        {
                            
                        }
                        break;
                    }
                case 8:
                    {
                        break;
                    }
            }
            if (reader.Remaining < payloadSize + 1) // last frame byte - end marker 206
            {
                return false;
            }
            return false;
        }
        public bool TryParseMethodFrame(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out MethodFrame methodFrame)
        {
            SequenceReader<byte> reader = new SequenceReader<byte>(input);
            methodFrame = default;
            return false;
        }
    }

}
