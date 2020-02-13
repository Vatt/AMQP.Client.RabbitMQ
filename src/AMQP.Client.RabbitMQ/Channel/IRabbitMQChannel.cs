using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    public interface IRabbitMQChannel
    {
        public ushort ChannelId { get; }
        public bool IsOpen { get; }
        Task<bool> TryOpenChannelAsync();
        Task<bool> TryCloseChannelAsync(string reason);
        Task<bool> TryCloseChannelAsync(short replyCode, string replyText, short failedClassId, short failedMethodId);
    }
    public interface IRabbitMQDefaultChannel:IRabbitMQChannel
    {
        ValueTask<bool> ExchangeDeclareAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null);
        ValueTask ExchangeDeclareNoWaitAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null);
        ValueTask<bool> ExchangeDeclarePassiveAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null);
        ValueTask<bool> ExchangeDeleteAsync(string name, bool ifUnused = false);
        ValueTask ExchangeDeleteNoWaitAsync(string name, bool ifUnused = false);
        ValueTask<QueueDeclareOk> QueueDeclareAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments);
        ValueTask<QueueDeclareOk> QueueDeclarePassiveAsync(string name);
        ValueTask QueueDeclareNoWaitAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments);

    }
    
}
