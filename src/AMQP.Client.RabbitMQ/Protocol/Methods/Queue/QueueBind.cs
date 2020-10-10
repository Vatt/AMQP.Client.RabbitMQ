using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct QueueBind
    {
        public readonly ushort ChannelId;
        public readonly string QueueName;
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly bool NoWait;
        public readonly Dictionary<string, object> Arguments;
        internal QueueBind(ushort channelId, string queueName, string exchangeName, string routingKey, bool noWait, Dictionary<string, object> arguments)
        {
            ChannelId = channelId;
            QueueName = queueName;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            NoWait = noWait;
            Arguments = arguments;
        }
        public static QueueBind Create(ushort channelId, string queueName, string exchangeName, string routingKey = "")
        {
            return new QueueBind(channelId, queueName, exchangeName, routingKey, false, null);
        }
        public static QueueBind Create(ushort channelId, string queueName, string exchangeName, Dictionary<string, object> arguments, string routingKey = "")
        {
            return new QueueBind(channelId, queueName, exchangeName, routingKey, false, arguments);
        }
        public static QueueBind CreateNoWait(ushort channelId, string queueName, string exchangeName, string routingKey = "")
        {
            return new QueueBind(channelId, queueName, exchangeName, routingKey, true, null);
        }
        public static QueueBind CreateNoWait(ushort channelId, string queueName, string exchangeName, Dictionary<string, object> arguments, string routingKey = "")
        {
            return new QueueBind(channelId, queueName, exchangeName, routingKey, true, arguments);
        }
    }
}
