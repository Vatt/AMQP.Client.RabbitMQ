namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public struct QueueDelete
    {
        public readonly ushort ChannelId;
        public readonly string Name;
        public readonly bool IfUnused;
        public readonly bool IfEmpty;
        public bool NoWait;
        internal QueueDelete(ushort channelId, string queueName, bool ifUnused, bool ifEmpty)
        {
            ChannelId = channelId;
            Name = queueName;
            IfUnused = ifUnused;
            IfEmpty = ifEmpty;
            NoWait = false;
        }
        public static QueueDelete Create(ushort channelId, string queueName, bool ifUnused = false, bool ifEmpty = false)
        { 
            return new QueueDelete(channelId, queueName, ifUnused, ifEmpty);
        }
    }
}
