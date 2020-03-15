namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public readonly struct QueueDeclareOk
    {
        public readonly string Name;
        public readonly int MessageCount;
        public readonly int ConsumerCount;
        public QueueDeclareOk(string name, int messageCount, int consumerCount)
        {
            Name = name;
            MessageCount = messageCount;
            ConsumerCount = consumerCount;
        }
    }
}
