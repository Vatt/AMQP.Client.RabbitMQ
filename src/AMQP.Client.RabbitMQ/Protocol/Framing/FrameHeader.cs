using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    public readonly struct FrameHeader
    {
        public readonly byte FrameType;
        public readonly short Chanell;
        public readonly int PaylodaSize;
        public FrameHeader(byte type, short chanell, int payloadSize)
        {
            FrameType = type;
            Chanell = chanell;
            PaylodaSize = payloadSize;
        }
    }
}
