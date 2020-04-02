namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public struct QueueDelete
    {
        public readonly string Name;
        public readonly bool IfUnused;
        public readonly bool IfEmpty;
        public bool NoWait;
        internal QueueDelete(string queueName, bool ifUnused, bool ifEmpty)
        {
            Name = queueName;
            IfUnused = ifUnused;
            IfEmpty = ifEmpty;
            NoWait = false;
        }
        public static QueueDelete Create(string queueName, bool ifUnused = false, bool ifEmpty = false) => new QueueDelete(queueName, ifUnused, ifEmpty);
    }
}
