using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Publisher
{
    public class RabbitMQPublisher
    { 
        private readonly ushort _channelId;
        private readonly RabbitMQProtocol _protocol;
        private readonly int _maxFrameSize;
        private readonly SemaphoreSlim _semaphore;
        internal RabbitMQPublisher(ushort channelId, RabbitMQProtocol protocol, int maxFrameSize, SemaphoreSlim semaphore)
        {
            _channelId = channelId;
            _protocol = protocol;
            _maxFrameSize = maxFrameSize;
            _semaphore = semaphore;
        }
        
        public ValueTask Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties, ReadOnlyMemory<byte> message)
        {
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(60, message.Length, ref properties);
            //return _protocol.Writer.WriteAsync(new PublishFastWriter(_channelId), (info, content, message));
            if (message.Length <= _maxFrameSize)
            {
                return _protocol.Writer.WriteAsync(new PublishFastWriter(_channelId), (info, content, message));
            }
            throw new NotImplementedException("message.Length > _maxFrameSize");

        }
    }
}
