using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct QueueUnbindInfo
    {
        public readonly string QueueName;
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly Dictionary<string, object> Arguments;
        public QueueUnbindInfo(string queueName, string exchangeName, string routingKey, Dictionary<string, object> arguments)
        {
            QueueName = queueName;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            Arguments = arguments;
        }
    }
}
