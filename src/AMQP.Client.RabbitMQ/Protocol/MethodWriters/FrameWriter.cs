using AMQP.Client.RabbitMQ.Protocol.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.MethodWriters
{
    internal class FrameWriter
    {
        public static void WriteFrameHeader(byte type, short chanell, int payloadSize, ref ValueWriter output)
        {
            output.WriteOctet(type);
            output.WriteShortInt(chanell);
            output.WriteLong(payloadSize);
        }
        public static void WriteMethodFrame(short classId, short methodId, ref ValueWriter output)
        {
            output.WriteShortInt(classId);
            output.WriteShortInt(methodId);
        }
    }
}
