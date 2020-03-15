using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public static class ConnectionProtocolExtension
    {
        private static ConnectionStartReader _connectionStartReader = new ConnectionStartReader();
        private static ConnectionTuneReader _connectionTuneReader = new ConnectionTuneReader();
        private static ConnectionOpenOkReader _connectionOpenOkReader = new ConnectionOpenOkReader();
        public static ValueTask SendStartOk(this RabbitMQProtocol protocol, RabbitMQClientInfo clientInfo, RabbitMQConnectionInfo connInfo)
        {
            return protocol.Writer.WriteAsync(new ConnectionStartOkWriter(connInfo), clientInfo);
        }
        public static ValueTask SendTuneOk(this RabbitMQProtocol protocol, RabbitMQMainInfo info)
        {
            return protocol.Writer.WriteAsync(new ConnectionTuneOkWriter(), info);
        }
        public static ValueTask SendOpen(this RabbitMQProtocol protocol, string vhost)
        {
            return protocol.Writer.WriteAsync(new ConnectionOpenWriter(), vhost);
        }
        public static async ValueTask<RabbitMQServerInfo> ReadStartAsync(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_connectionStartReader).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            protocol.Reader.Advance();
            return result.Message;
        }
        public static async ValueTask<RabbitMQMainInfo> ReadTuneMethodAsync(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_connectionTuneReader).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            protocol.Reader.Advance();
            return result.Message;
        }
        public static async ValueTask<bool> ReadConnectionOpenOkAsync(this RabbitMQProtocol protocol)
        {
            var result = await protocol.Reader.ReadAsync(_connectionOpenOkReader).ConfigureAwait(false);
            if (result.IsCompleted)
            {
                //TODO: сделать чтонибудь
            }
            var isOpen = result.Message;
            protocol.Reader.Advance();
            return isOpen;

        }
        public static ValueTask SendConnectionCloseAsync(this RabbitMQProtocol protocol, CloseInfo info)
        {
            return protocol.SendClose(0, 10, 50, info);
        }
        public static ValueTask SendConnectionCloseOkAsync(this RabbitMQProtocol protocol)
        {
            return protocol.SendCloseOk(1, 0, 10, 51);
        }
    }
}
