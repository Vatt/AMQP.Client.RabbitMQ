namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public struct QueuePurge
    {
        public readonly ushort ChannelId;
        public readonly string Name;
        public bool NoWait;
        internal QueuePurge(ushort channelId, string queueName, bool noWait = false)
        {
            ChannelId = channelId;
            Name = queueName;
            NoWait = noWait;
        }

        public static QueuePurge Create(ushort channelId, string queueName) => new QueuePurge(channelId, queueName);
    }
}
