using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct QueueUnbind
    {
        public readonly ushort ChannelId;
        public readonly string QueueName;
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly Dictionary<string, object> Arguments;
        internal QueueUnbind(ushort channelId, string queueName, string exchangeName, string routingKey, Dictionary<string, object> arguments)
        {
            ChannelId = channelId;
            QueueName = queueName;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            Arguments = arguments;
        }
        public static QueueUnbind Create(ushort channelId, string queueName, string exchangeName)
        {
            return new QueueUnbind(channelId, queueName, exchangeName, string.Empty, null);
        }
        public static QueueUnbind Create(ushort channelId, string queueName, string exchangeName, string routingKey)
        {
            return new QueueUnbind(channelId, queueName, exchangeName, routingKey, null);
        }
        public static QueueUnbind Create(ushort channelId, string queueName, string exchangeName, string routingKey, Dictionary<string, object> arguments)
        {
            return new QueueUnbind(channelId, queueName, exchangeName, routingKey, arguments);
        }
        public static QueueUnbind Create(ushort channelId, string queueName, string exchangeName, Dictionary<string, object> arguments)
        {
            return new QueueUnbind(channelId, queueName, exchangeName, string.Empty, arguments);
        }
    }
}
