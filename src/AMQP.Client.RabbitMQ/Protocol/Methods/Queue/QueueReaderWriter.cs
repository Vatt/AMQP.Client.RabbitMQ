using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        public async ValueTask SendQueueDeclare(QueueInfo info)
        {
            await _protocol.Writer.WriteAsync(new QueueDeclareWriter(ChannelId), info);
        }
        public async ValueTask<QueueDeclareOk> ReadQueueDeclareOk()
        {
            var result = await _protocol.Reader.ReadAsync(new QueueDeclareOkReader());
            _protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }
        public async ValueTask SendQueueBind(QueueBindInfo info)
        {
            await _protocol.Writer.WriteAsync(new QueueBindWriter(ChannelId), info);
        }
        
        public async ValueTask SendQueueUnbind(QueueUnbindInfo info)
        {
            await _protocol.Writer.WriteAsync(new QueueUnbindWriter(ChannelId), info);
        }
        public async ValueTask SendQueuePurge(QueuePurgeInfo info)
        {
            await _protocol.Writer.WriteAsync(new QueuePurgeWriter(ChannelId), info);
        }
        public async ValueTask SendQueueDelete(QueueDeleteInfo info)
        {
            await _protocol.Writer.WriteAsync(new QueueDeleteWriter(ChannelId), info);
        }
        public async ValueTask<int> ReadQueuePurgeOkDeleteOk()
        {
            var result = await _protocol.Reader.ReadAsync(new QueuePurgeOkDeleteOkReader());
            _protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }
        public async ValueTask<bool> ReadBindOkUnbindOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader());
            _protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }

    }
}
