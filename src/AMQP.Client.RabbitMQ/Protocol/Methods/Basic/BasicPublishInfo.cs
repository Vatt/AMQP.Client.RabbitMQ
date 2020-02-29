namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct BasicPublishInfo
    {
        public readonly string ExchangeName;
        public readonly string RoutingKey;
        public readonly bool Mandatory;
        public readonly bool Immediate;
        public BasicPublishInfo(string exchangeName, string routingKey, bool mandatory, bool immediate)
        {
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
            Mandatory = mandatory;
            Immediate = immediate;
        }
    }
}
