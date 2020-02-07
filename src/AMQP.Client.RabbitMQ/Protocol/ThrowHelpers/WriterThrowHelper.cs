using System;
using System.Collections.Generic;
using System.Text;

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
