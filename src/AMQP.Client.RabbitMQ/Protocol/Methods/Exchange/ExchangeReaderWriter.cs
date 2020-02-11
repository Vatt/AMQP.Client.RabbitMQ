using AMQP.Client.RabbitMQ.Protocol.Info.Exchange;
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
        public async ValueTask WriteExchangeDeclareAsync(ExchangeInfo message)
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
    }
}
