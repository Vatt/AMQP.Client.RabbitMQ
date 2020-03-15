using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Common;

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
            var result = await _protocol.Reader.ReadAsync(new MethodHeaderReader()).ConfigureAwait(false);
            _protocol.Reader.Advance();
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            return result.Message;
        }
        public ValueTask SendStartOk(RabbitMQClientInfo clientInfo, RabbitMQConnectionInfo connInfo)
        {
            return _protocol.Writer.WriteAsync(new ConnectionStartOkWriter(connInfo), clientInfo);
        }
        public ValueTask SendTuneOk(RabbitMQMainInfo info)
        {
            return _protocol.Writer.WriteAsync(new ConnectionTuneOkWriter(), info);
        }
        public ValueTask SendOpen(string vhost)
        {
            return _protocol.Writer.WriteAsync(new ConnectionOpenWriter(), vhost);
        }
        public async ValueTask<RabbitMQServerInfo> ReadStartAsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionStartReader()).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        public async ValueTask<bool> ReadOpenOkAsync()
        {
            var result = await _protocol.Reader.ReadAsync(new ConnectionOpenOkReader()).ConfigureAwait(false);
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
            var result = await _protocol.Reader.ReadAsync(new ConnectionTuneReader()).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        public async ValueTask<CloseInfo> ReadCloseMethod()
        {
            var result = await _protocol.Reader.ReadAsync(new CloseReader()).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        public ValueTask SendCloseMethodAsync(CloseInfo info)
        {
            return _protocol.Writer.WriteAsync(new CloseWriter(0, 10, 50), info);
        }
        public async ValueTask<bool> ReadCloseOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader()).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
        public ValueTask SendCloseOk()
        {
            return _protocol.Writer.WriteAsync(new NoPayloadMethodWrtier(), new NoPaylodMethodInfo(1, 0, 10, 51));
        }
    }
}
