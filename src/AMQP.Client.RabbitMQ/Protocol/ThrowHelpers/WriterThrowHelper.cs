using System;

namespace AMQP.Client.RabbitMQ.Protocol.ThrowHelpers
{
    internal class WriterThrowHelper
    {
        public static void ThrowIfValueWriterOutOfRange()
        {
            throw new IndexOutOfRangeException("ValueWriter");
        }
    }
}
