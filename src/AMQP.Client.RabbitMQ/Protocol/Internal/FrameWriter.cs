using AMQP.Client.RabbitMQ.Protocol.Internal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Internal
{
    internal class FrameWriter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteFrameHeader(byte type, ushort channel, int payloadSize, ref ValueWriter output)
        {
            output.WriteOctet(type);
            output.WriteShortInt(channel);
            output.WriteLong(payloadSize);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteMethodFrame(short classId, short methodId, ref ValueWriter output)
        {
            output.WriteShortInt(classId);
            output.WriteShortInt(methodId);
        }
    }
}
