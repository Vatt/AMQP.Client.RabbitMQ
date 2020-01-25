using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Framing
{
    internal struct Frame
    {
        public readonly byte FrameType;
        public readonly short Chanell;
        public readonly int PaylodaSize;
        public Frame(byte type, short chanell, int payloadSize)
        {
            FrameType = type;
            Chanell = chanell;
            PaylodaSize = payloadSize;
        }
    }
}
