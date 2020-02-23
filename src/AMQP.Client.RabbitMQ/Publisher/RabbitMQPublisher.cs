using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Publisher
{
    public class RabbitMQPublisher
    {
        //Может быть нужно локнуться?
        private readonly ushort _channelId;
        private readonly RabbitMQProtocol _protocol;
        private readonly int _maxFrameSize;
        internal RabbitMQPublisher(ushort channelId, RabbitMQProtocol protocol, int maxFrameSize)
        {
            _channelId = channelId;
            _protocol = protocol;
            _maxFrameSize = maxFrameSize;
        }
        
        public async ValueTask Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties , Action<IBufferWriter<byte>> callback)
        {
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(60, 0, ref properties);

            await _protocol.Writer.WriteAsync(new BasicPublishWriter(_channelId), info).ConfigureAwait(false);
            await _protocol.Writer.WriteAsync(new ContentHeaderWriter(_channelId), content).ConfigureAwait(false);
            
        }

        public async ValueTask Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties, ReadOnlyMemory<byte> message)
        {
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(60, message.Length, ref properties);
            await _protocol.Writer.WriteAsync(new BasicPublishWriter(_channelId), info).ConfigureAwait(false);
            await _protocol.Writer.WriteAsync(new ContentHeaderWriter(_channelId), content).ConfigureAwait(false);
            if (content.BodySize < _maxFrameSize)
            {
                await _protocol.Writer.WriteAsync(new BodyFrameWriter(_channelId), message).ConfigureAwait(false);
            }
            else
            {
                long written = 0;
                while(written < content.BodySize)
                {
                    var writable = Math.Min(_maxFrameSize, content.BodySize - written);
                    await _protocol.Writer.WriteAsync(new BodyFrameWriter(_channelId), message.Slice((int)written, (int)writable));
                    written += writable;
                }
            }
        }
    }
}
