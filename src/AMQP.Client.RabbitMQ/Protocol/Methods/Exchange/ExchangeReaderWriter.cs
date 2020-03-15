using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public class ExchangeReaderWriter
    {
        protected readonly RabbitMQProtocol _protocol;
        private readonly ushort _channelId;
        public ExchangeReaderWriter(ushort channelId, RabbitMQProtocol protocol)
        {
            _channelId = channelId;
            _protocol = protocol;
        }
        public ValueTask SendExchangeDeclareAsync(ExchangeInfo message)
        {
            return _protocol.Writer.WriteAsync(new ExchangeDeclareWriter(_channelId), message);
        }
        public async ValueTask<bool> ReadExchangeDeclareOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader()).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        public ValueTask SendExchangeDeleteAsync(ExchangeDeleteInfo info)
        {
            return _protocol.Writer.WriteAsync(new ExchangeDeleteWriter(_channelId), info);
        }
        public async ValueTask<bool> ReadExchangeDeleteOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader()).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
    }
}
