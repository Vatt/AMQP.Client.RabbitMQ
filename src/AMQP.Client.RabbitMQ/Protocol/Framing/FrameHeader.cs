namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    public readonly struct FrameHeader
    {
        public readonly byte FrameType;
        public readonly ushort Channel;
        public readonly int PayloadSize;

        public FrameHeader(byte type, ushort chanell, int payloadSize)
        {
            FrameType = type;
            Channel = chanell;
            PayloadSize = payloadSize;
        }
    }
}