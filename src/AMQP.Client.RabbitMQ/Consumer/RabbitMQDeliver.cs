using AMQP.Client.RabbitMQ.Protocol.Framing;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public readonly struct RabbitMQDeliver
    {
        public readonly ContentHeader Header;
        public readonly long DeliveryTag;

        internal RabbitMQDeliver(long deliveryTag, ContentHeader header)
        {
            Header = header;
            DeliveryTag = deliveryTag;
        }
    }
}