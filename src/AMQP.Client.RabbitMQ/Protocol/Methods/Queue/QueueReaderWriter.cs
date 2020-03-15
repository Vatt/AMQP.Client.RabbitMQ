using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Queue
{
    public class QueueReaderWriter
    {
        protected readonly RabbitMQProtocol _protocol;
        private readonly ushort ChannelId;
        public QueueReaderWriter(ushort channelId, RabbitMQProtocol protocol)
        {
            _protocol = protocol;
            ChannelId = channelId;
        }
        public ValueTask SendQueueDeclare(QueueInfo info)
        {
            return _protocol.Writer.WriteAsync(new QueueDeclareWriter(ChannelId), info);
        }
        public async ValueTask<QueueDeclareOk> ReadQueueDeclareOk()
        {
            var result = await _protocol.Reader.ReadAsync(new QueueDeclareOkReader()).ConfigureAwait(false);
            _protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }
        public ValueTask SendQueueBind(QueueBindInfo info)
        {
            return _protocol.Writer.WriteAsync(new QueueBindWriter(ChannelId), info);
        }

        public ValueTask SendQueueUnbind(QueueUnbindInfo info)
        {
            return _protocol.Writer.WriteAsync(new QueueUnbindWriter(ChannelId), info);
        }
        public ValueTask SendQueuePurge(QueuePurgeInfo info)
        {
            return _protocol.Writer.WriteAsync(new QueuePurgeWriter(ChannelId), info);
        }
        public ValueTask SendQueueDelete(QueueDeleteInfo info)
        {
            return _protocol.Writer.WriteAsync(new QueueDeleteWriter(ChannelId), info);
        }
        public async ValueTask<int> ReadQueuePurgeOkDeleteOk()
        {
            var result = await _protocol.Reader.ReadAsync(new QueuePurgeOkDeleteOkReader()).ConfigureAwait(false);
            _protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }
        public async ValueTask<bool> ReadBindOkUnbindOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader()).ConfigureAwait(false);
            _protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }

    }
}
