using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct QueueBindInfo
    {
        public readonly string QueueName;
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly bool NoWait;
        public readonly Dictionary<string, object> Arguments;
        public QueueBindInfo(string queueName,string exchangeName,string routingKey, bool noWait, Dictionary<string, object> arguments)
        {
            QueueName = queueName;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            NoWait = noWait;
            Arguments = arguments;
        }
    }
}
