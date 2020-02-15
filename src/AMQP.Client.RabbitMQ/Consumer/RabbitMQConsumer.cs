using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public class  RabbitMQConsumer
    {
        public readonly string ConsumerTag;

        public event Action<byte[]> Received;
        public event Action Close;
    }
}
