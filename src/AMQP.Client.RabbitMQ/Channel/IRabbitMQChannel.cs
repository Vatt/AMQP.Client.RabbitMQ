using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Info;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    public interface IRabbitMQChannel
    {
        public short ChannelId { get; }
        public bool IsOpen { get; }

        ValueTask HandleAsync(FrameHeader header);
        Task<bool> TryOpenChannelAsync();
        Task<bool> TryCloseChannelAsync(string reason);
        Task<bool> TryCloseChannelAsync(short replyCode, string replyText, short failedClassId, short failedMethodId);
    }
}
