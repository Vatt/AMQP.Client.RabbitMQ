namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public struct QueuePurge
    {
        public readonly string Name;
        public bool NoWait;
        internal QueuePurge(string queueName, bool noWait = false)
        {
            Name = queueName;
            NoWait = noWait;
        }

        public static QueuePurge Create(string queueName) => new QueuePurge(queueName);
    }
}
