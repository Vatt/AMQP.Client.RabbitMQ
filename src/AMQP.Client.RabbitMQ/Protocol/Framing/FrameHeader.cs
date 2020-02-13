using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Protocol.Framing
{
    public readonly struct FrameHeader
    {
        public readonly byte FrameType;
        public readonly ushort Channel;
        public readonly int PaylodaSize;
        public FrameHeader(byte type, ushort chanell, int payloadSize)
        {
            FrameType = type;
            Channel = chanell;
            PaylodaSize = payloadSize;
        }
    }
}
