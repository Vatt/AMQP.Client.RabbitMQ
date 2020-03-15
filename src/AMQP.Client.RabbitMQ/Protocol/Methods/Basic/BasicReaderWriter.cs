using System.Collections.Generic;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Basic
{
    public class BasicReaderWriter
    {
        protected readonly RabbitMQProtocol _protocol;
        protected readonly ushort _channelId;
        private readonly BasicDeliverReader _basicDeliverReader;
        private readonly ShortStrPayloadReader _shortStrPayloadReader;

        public BasicReaderWriter(ushort channelId, RabbitMQProtocol protocol)
        {
            _channelId = channelId;
            _protocol = protocol;
            _basicDeliverReader = new BasicDeliverReader();
            _shortStrPayloadReader = new ShortStrPayloadReader();
        }
        public ValueTask SendBasicConsume(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                          bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            var info = new ConsumerInfo(queueName, consumerTag, noLocal, noAck, exclusive, false, arguments);
            return _protocol.Writer.WriteAsync(new BasicConsumeWriter(_channelId), info);
        }
        public async ValueTask<string> ReadBasicConsumeOk()
        {
            var result = await _protocol.Reader.ReadAsync(_shortStrPayloadReader).ConfigureAwait(false);
            _protocol.Reader.Advance();
            if (!result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            return result.Message;
        }
        public async ValueTask<DeliverInfo> ReadBasicDeliver()
        {
            var result = await _protocol.Reader.ReadAsync(_basicDeliverReader).ConfigureAwait(false);
            _protocol.Reader.Advance();
            if (!result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            return result.Message;

        }
        public ValueTask SendBasicQoS(ref QoSInfo info)
        {
            return _protocol.Writer.WriteAsync(new BasicQoSWriter(_channelId), info);
        }
        public async ValueTask<bool> ReadBasicQoSOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader()).ConfigureAwait(false);
            _protocol.Reader.Advance();
            if (result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            return result.Message;
        }
    }
}
