using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Encoder
{
    internal class EncoderThrowHelper
    {
        public static void ThrowValueWriterOutOfRange()
        {
            throw new IndexOutOfRangeException("ValueWriter");
        }
    }
}
