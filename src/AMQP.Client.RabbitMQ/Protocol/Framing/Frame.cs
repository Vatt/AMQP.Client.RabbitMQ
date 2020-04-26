using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    internal struct Frame
    {
        public FrameHeader Header;
        public ReadOnlySequence<byte> Payload;

        public Frame(byte type, ushort chanellId, int payloadSize, ReadOnlySequence<byte> payload)
        {
            Header = new FrameHeader(type, chanellId, payloadSize);
            Payload = payload;
        }

        public Frame(FrameHeader header, ReadOnlySequence<byte> payload)
        {
            Header = header;
            Payload = payload;
        }
    }
}