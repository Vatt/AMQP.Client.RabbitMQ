using System;

namespace AMQP.Client.RabbitMQ.Protocol.Exceptions
{
    public class RabbitMQException : Exception
    {
        public RabbitMQException(string? message) : base(message)
        {

        }
        public RabbitMQException(string? message, Exception? innerException) : base(message, innerException)
        {

        }
    }
}
