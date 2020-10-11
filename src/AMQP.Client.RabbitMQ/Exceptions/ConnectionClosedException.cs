using System;

namespace AMQP.Client.RabbitMQ.Exceptions
{
    public class ConnectionClosedException : InvalidOperationException
    {
        public ConnectionClosedException(Guid id) : base($"Connection {id} already closed")
        {
            
        }
    }
}