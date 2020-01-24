using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Framing
{
    internal readonly struct MethodFrame
    {
        public readonly short ClassId;
        public readonly short MethodId;
        public MethodFrame(short classId, short methodId)
        {
            ClassId = classId;
            MethodId = methodId;
        }
    }
}
