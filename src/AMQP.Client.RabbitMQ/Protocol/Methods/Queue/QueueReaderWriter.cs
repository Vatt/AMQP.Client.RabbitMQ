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
        public async ValueTask<QueueDeclareOk> ReadDeclareOk()
        {
            var result = await _protocol.Reader.ReadAsync(new QueueDeclareOkReader());
            _protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO:Сделать что нибудь
            }
            return result.Message;
        }
    }
}
