using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct QueueBind
    {
        public readonly string QueueName;
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly bool NoWait;
        public readonly Dictionary<string, object> Arguments;
        internal QueueBind(string queueName, string exchangeName, string routingKey, bool noWait, Dictionary<string, object> arguments)
        {
            QueueName = queueName;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            NoWait = noWait;
            Arguments = arguments;
        }
        public static QueueBind Create(string queueName, string exchangeName, string routingKey = "")
        {
            return new QueueBind(queueName, exchangeName, routingKey, false, null);
        }
        public static QueueBind Create(string queueName, string exchangeName, Dictionary<string, object> arguments, string routingKey = "")
        {
            return new QueueBind(queueName, exchangeName, routingKey, false, arguments);
        }
        public static QueueBind CreateNoWait(string queueName, string exchangeName, string routingKey = "")
        {
            return new QueueBind(queueName, exchangeName, routingKey, true, null);
        }
        public static QueueBind CreateNoWait(string queueName, string exchangeName, Dictionary<string, object> arguments, string routingKey = "")
        {
            return new QueueBind(queueName, exchangeName, routingKey, true, arguments);
        }
    }
}
