namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public struct Deliver
    {
        public readonly string ConsumerTag;
        public readonly long DeliverTag;
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly bool Redelivered;
        public Deliver(string consumerTag, long deliverTag, bool redelivered, string exchangeName, string routingKey)
        {
            ConsumerTag = consumerTag;
            DeliverTag = deliverTag;
            Redelivered = redelivered;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
        }
    }
}
