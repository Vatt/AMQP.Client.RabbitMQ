using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Exchange
{
    public class ExchangeReaderWriter
    {
        protected readonly RabbitMQProtocol _protocol;
        public ExchangeReaderWriter(RabbitMQProtocol protocol)
        {
            _protocol = protocol;
        }
        public async ValueTask SendExchangeDeclareAsync(ExchangeInfo message)
        {
            await _protocol.Writer.WriteAsync(new ExchangeDeclareWriter(), message);
        }
        public async ValueTask<bool> ReadExchangeDeclareOk()
        {
            var result = await _protocol.Reader.ReadAsync(new ExchangeDeclareOkReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        public async ValueTask SendExchangeDeleteAsync(ExchangeDeleteInfo info)
        {
            await _protocol.Writer.WriteAsync(new ExchangeDeleteWriter(), info);
        }
        public async ValueTask<bool> ReadExchangeDeleteOk()
        {
            var result = await _protocol.Reader.ReadAsync(new ExchangeDeleteOkReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
    }
}
