using System.Threading.Tasks;

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
        public async ValueTask SendExchangeDeclareAsync(ExchangeInfo message)
        {
            await _protocol.Writer.WriteAsync(new ExchangeDeclareWriter(_channelId), message);
        }
        public async ValueTask<bool> ReadExchangeDeclareOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        public async ValueTask SendExchangeDeleteAsync(ExchangeDeleteInfo info)
        {
            await _protocol.Writer.WriteAsync(new ExchangeDeleteWriter(_channelId), info);
        }
        public async ValueTask<bool> ReadExchangeDeleteOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
    }
}
