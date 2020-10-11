using System;
using AMQP.Client.RabbitMQ.Protocol.Common;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQCloseArgs : EventArgs
    {
        public Exception? Exception { get; }
        public CloseInfo? Info { get; }
        public RabbitMQCloseArgs(CloseInfo? info, Exception? exception)
        {
            Info = info;
            Exception = exception;
        }
    }
}