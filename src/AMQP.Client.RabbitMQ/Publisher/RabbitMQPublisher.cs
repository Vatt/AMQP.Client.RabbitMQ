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
        internal RabbitMQPublisher(ushort channelId, RabbitMQProtocol protocol)
        {
            _channelId = channelId;
            _protocol = protocol;
        }
        
        public async ValueTask Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties , Action<IBufferWriter<byte>> callback)
        {
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            
            var writer = _protocol.Context.Transport.Output;
            await _protocol.Writer.WriteAsync(new BasicPublishWriter(_channelId), info);
            callback(writer);
            var oneByte = writer.GetMemory(1);
            oneByte.Span[0] = Constants.FrameEnd;
            await writer.FlushAsync();

        }
        public async ValueTask Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties, byte[] message)
        {
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(60, 0, message.Length);
            content.SetProperties(properties);
            await _protocol.Writer.WriteAsync(new BasicPublishWriter(_channelId), info);
            await _protocol.Writer.WriteAsync(new ContentHeaderWriter(_channelId), content);
            await _protocol.Writer.WriteAsync(new BodyFrameWriter(_channelId), message);
        }
    }
}
