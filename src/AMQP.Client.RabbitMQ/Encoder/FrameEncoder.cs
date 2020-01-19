using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Encoder
{
    internal class FrameEncoder
    {
        public static void EncodeStartOkFrame(Memory<byte> destination)
        {
            ValueWriter encoder = new ValueWriter(destination);
            EncodeFrameHeader(1, 0, 4, ref encoder);
        }
        public static void EncodeFrameHeader(byte type, short chanell, int payloadSize, ref ValueWriter encoder)
        {
            encoder.WriteOctet(type);
            encoder.WriteShortInt(chanell);
            encoder.WriteLong(payloadSize);
        }
    }
}
