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
        private readonly BasicPublishWriter _publishWriter;
        private readonly ContentHeaderWriter _contentWriter;
        private readonly BodyFrameWriter _bodyFrameWriter;
        internal RabbitMQPublisher(ushort channelId, RabbitMQProtocol protocol)
        {
            _channelId = channelId;
            _protocol = protocol;
            _publishWriter = new BasicPublishWriter(_channelId);
            _contentWriter = new ContentHeaderWriter(_channelId);
            _bodyFrameWriter = new BodyFrameWriter(_channelId);
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
            await _protocol.Writer.WriteAsync(_publishWriter, info);
            await _protocol.Writer.WriteAsync(_contentWriter, content);
            await _protocol.Writer.WriteAsync(_bodyFrameWriter, message);
            //_publishWriter.WriteMessage(info, _protocol.Context.Transport.Output);
            //_contentWriter.WriteMessage(content, _protocol.Context.Transport.Output);
            //_bodyFrameWriter.WriteMessage(message, _protocol.Context.Transport.Output);
            //await _protocol.Context.Transport.Output.FlushAsync();
        }
    }
}
