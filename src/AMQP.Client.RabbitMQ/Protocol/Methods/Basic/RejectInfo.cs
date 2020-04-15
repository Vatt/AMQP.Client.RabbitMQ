namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public readonly struct RejectInfo
    {
        public readonly long DeliveryTag;
        public readonly bool Requeue;
        internal RejectInfo(long tag, bool requeue)
        {
            DeliveryTag = tag;
            Requeue = requeue;
        }
        public static RejectInfo Create(long deliveryTag, bool requeue) => new RejectInfo(deliveryTag, requeue);
    }
}
