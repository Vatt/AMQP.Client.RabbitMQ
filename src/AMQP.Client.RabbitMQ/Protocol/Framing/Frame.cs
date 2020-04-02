using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    public readonly struct Frame
    {
        public readonly FrameHeader Header;
        public readonly ReadOnlySequence<byte> Payload;
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
