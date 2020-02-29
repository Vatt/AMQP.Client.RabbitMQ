namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct AckInfo
    {
        public readonly long DeliveryTag;
        public readonly bool Multiple;
        public AckInfo(long deliveryTag, bool multiple)
        {
            DeliveryTag = deliveryTag;
            Multiple = multiple;
        }
    }
}
