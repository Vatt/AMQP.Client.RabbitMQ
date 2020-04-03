using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct QueueUnbind
    {
        public readonly string QueueName;
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly Dictionary<string, object> Arguments;
        internal QueueUnbind(string queueName, string exchangeName, string routingKey, Dictionary<string, object> arguments)
        {
            QueueName = queueName;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            Arguments = arguments;
        }
        public static QueueUnbind Create(string queueName, string exchangeName)
        {
            return new QueueUnbind(queueName, exchangeName, string.Empty, null);
        }
        public static QueueUnbind Create(string queueName, string exchangeName, string routingKey)
        {
            return new QueueUnbind(queueName, exchangeName, routingKey, null);
        }
        public static QueueUnbind Create(string queueName, string exchangeName, string routingKey, Dictionary<string, object> arguments)
        {
            return new QueueUnbind(queueName, exchangeName, routingKey, arguments);
        }
        public static QueueUnbind Create(string queueName, string exchangeName, Dictionary<string, object> arguments)
        {
            return new QueueUnbind(queueName, exchangeName, string.Empty, arguments);
        }
    }
}
