namespace AMQP.Client.RabbitMQ.Channel
{
    /*
    public interface IRabbitMQClosable
    {
        public bool IsClosed { get; }
        Task<bool> CloseAsync(string reason);
        Task<bool> CloseAsync(short replyCode, string replyText, short failedClassId, short failedMethodId);
    }
    internal interface IRabbitMQOpenable
    {
        Task OpenAsync(RabbitMQProtocolWriter protocol);
    }
    internal interface IChannel : IRabbitMQOpenable, IRabbitMQClosable
    {
        ValueTask HandleFrameHeaderAsync(FrameHeader header);
        Task<CloseInfo> WaitClose();
    }
    public interface IRabbitMQChannel : IRabbitMQClosable
    {
        public ushort ChannelId { get; }
        ValueTask<bool> ExchangeDeclareAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null);
        ValueTask ExchangeDeclareNoWaitAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null);
        ValueTask<bool> ExchangeDeclarePassiveAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null);
        ValueTask<bool> ExchangeDeleteAsync(string name, bool ifUnused = false); //TODO:починить
        ValueTask ExchangeDeleteNoWaitAsync(string name, bool ifUnused = false); //TODO:починить
        ValueTask<QueueDeclareOk> QueueDeclareAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments);
        ValueTask<QueueDeclareOk> QueueDeclarePassiveAsync(string name);
        ValueTask QueueDeclareNoWaitAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments);
        ValueTask<QueueDeclareOk> QueueDeclareQuorumAsync(string name);
        ValueTask<bool> QueueBindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null);
        ValueTask QueueBindNoWaitAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null);
        ValueTask<bool> QueueUnbindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null);
        ValueTask<int> QueuePurgeAsync(string queueName);
        ValueTask QueuePurgeNoWaitAsync(string queueName);
        ValueTask QueueDeleteNoWaitAsync(string queueName, bool ifUnused = false, bool ifEmpty = false);
        ValueTask<int> QueueDeleteAsync(string queueName, bool ifUnused = false, bool ifEmpty = false);

        RabbitMQChunkedConsumer CreateChunkedConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                                 bool exclusive = false, Dictionary<string, object> arguments = null);
        RabbitMQConsumer CreateConsumer(string queueName, string consumerTag, PipeScheduler scheduler, bool noLocal = false, bool noAck = false,
                                                   bool exclusive = false, Dictionary<string, object> arguments = null);
        ValueTask Ack(long deliveryTag, bool multiple = false);
        ValueTask Reject(long deliveryTag, bool requeue);

        ValueTask QoS(int prefetchSize, ushort prefetchCount, bool global);

        ValueTask Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties, ReadOnlyMemory<byte> body);
    }
    */
}
