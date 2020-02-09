using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Info;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public class ConnectionReaderWriter
    {
        protected readonly RabbitMQProtocol _protocol;
        public ConnectionReaderWriter(RabbitMQProtocol protocol)
        {
            _protocol = protocol;
        }
        public async ValueTask<MethodHeader> ReadMethodHeader()
        {
            var result = await _protocol.Reader.ReadAsync(new MethodHeaderReader());
            _protocol.Reader.Advance();
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            return result.Message;
        }
        public async ValueTask SendStartOk(RabbitMQClientInfo clientInfo, RabbitMQConnectionInfo connInfo)
        {
            var startok = new ConnectionStartOkWriter(connInfo);
            await _protocol.Writer.WriteAsync(startok, clientInfo);
        }
        public async ValueTask SendTuneOk(RabbitMQMainInfo info)
        {
            var tuneok = new ConnectionTuneOkWriter();
            await _protocol.Writer.WriteAsync(tuneok, info);
        }
        public async ValueTask SendOpen(string vhost)
        {
            var open = new ConnectionOpenWriter();
            await _protocol.Writer.WriteAsync(open, vhost);
        }
        public async ValueTask<RabbitMQServerInfo> ReadStartAsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionStartReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        public async ValueTask<bool> ReadOpenOkAsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionOpenOkReader());
            if (result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            var isOpen = result.Message;
            _protocol.Reader.Advance();
            return isOpen;

        }
        public async ValueTask<RabbitMQMainInfo> ReadTuneMethodAsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionTuneReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
    }
}
