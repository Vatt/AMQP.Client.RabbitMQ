using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public class BasicReaderWriter
    {
        protected readonly RabbitMQProtocol _protocol;
        protected readonly ushort _channelId;
        public BasicReaderWriter(ushort channelId, RabbitMQProtocol protocol)
        {
            _channelId = channelId;
            _protocol = protocol;
        }
        public async ValueTask SendBasicConsume(string queue, string tag)
        {
            var info = new ConsumerInfo(queue, tag, nowait: true);
            await _protocol.Writer.WriteAsync(new BasicConsumeWriter(_channelId), info);
        }
        public async ValueTask<DeliverInfo> ReadBasicDeliver()
        {
            var result = await _protocol.Reader.ReadAsync(new BasicDeliverReader());
            _protocol.Reader.Advance();
            if (!result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            return result.Message;

        }
    }
}
