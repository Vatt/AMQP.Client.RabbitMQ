namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct AckInfo
    {
        public readonly long DeliveryTag;
        public readonly bool Multiple;
        internal AckInfo(long deliveryTag, bool multiple)
        {
            DeliveryTag = deliveryTag;
            Multiple = multiple;
        }
        public static AckInfo Create(long deliveryTag, bool multiple = false) => new AckInfo(deliveryTag, multiple);
    }
}
