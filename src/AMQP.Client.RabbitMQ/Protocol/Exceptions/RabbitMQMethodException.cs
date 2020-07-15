using System;

namespace AMQP.Client.RabbitMQ.Protocol.Exceptions
{
    public class RabbitMQMethodException : RabbitMQException
    {
        public RabbitMQMethodException(string method, short classId, short methodId) : base($"{method} : cannot read frame (class-id,method-id):({classId}, {methodId})")
        {

        }
    }
}
