using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Chanell
{
    public abstract class RabbitMQChanell
    {
        public readonly short Id;
        public RabbitMQChanell(short id)
        {
            Id = id;
        }
        public bool TryOpenChanel()
        {
            return false;
        }
    }
}
