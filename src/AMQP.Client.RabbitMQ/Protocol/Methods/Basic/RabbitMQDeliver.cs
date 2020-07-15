namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public struct RabbitMQDeliver
    {
        public readonly string ConsumerTag;
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly long DeliverTag;
        public readonly bool Redelivered;
        public RabbitMQDeliver(string consumerTag, string exchangeName, string routingKey, long deliverTag, bool redelivered)
        {
            ConsumerTag = consumerTag;
            DeliverTag = deliverTag;
            Redelivered = redelivered;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
        }
    }
}
