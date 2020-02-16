using AMQP.Client.RabbitMQ.Protocol.Common;
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
        public async ValueTask SendBasicConsume(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            var info = new ConsumerInfo(queueName, consumerTag,noLocal,noAck,exclusive,false,arguments);
            await _protocol.Writer.WriteAsync(new BasicConsumeWriter(_channelId), info);
        }
        public async ValueTask<string> ReadBasicConsumeOk()
        {
            var result = await _protocol.Reader.ReadAsync(new ShortStrPayloadReader());
            _protocol.Reader.Advance();
            if (!result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            return result.Message;
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
