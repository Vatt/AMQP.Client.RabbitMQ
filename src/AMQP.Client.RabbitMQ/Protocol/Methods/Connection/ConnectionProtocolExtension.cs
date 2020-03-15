using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public static class ConnectionProtocolExtension
    {
        private static ConnectionStartReader _connectionStartReader = new ConnectionStartReader();
        private static ConnectionTuneReader _connectionTuneReader = new ConnectionTuneReader();
        private static ConnectionOpenOkReader _connectionOpenOkReader = new ConnectionOpenOkReader();
        public static ValueTask SendStartOk(this RabbitMQProtocol protocol, RabbitMQClientInfo clientInfo, RabbitMQConnectionInfo connInfo, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ConnectionStartOkWriter(connInfo), clientInfo, token);
        }
        public static ValueTask SendTuneOk(this RabbitMQProtocol protocol, RabbitMQMainInfo info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ConnectionTuneOkWriter(), info, token);
        }
        public static ValueTask SendOpen(this RabbitMQProtocol protocol, string vhost, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ConnectionOpenWriter(), vhost, token);
        }
        public static ValueTask<RabbitMQServerInfo> ReadStartAsync(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_connectionStartReader, token);
        }
        public static ValueTask<RabbitMQMainInfo> ReadTuneMethodAsync(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_connectionTuneReader, token);
        }
        public static ValueTask<bool> ReadConnectionOpenOkAsync(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_connectionOpenOkReader, token);

        }
        public static ValueTask SendConnectionCloseAsync(this RabbitMQProtocol protocol, CloseInfo info, CancellationToken token = default)
        {
            return protocol.SendClose(0, 10, 50, info, token);
        }
        public static ValueTask SendConnectionCloseOkAsync(this RabbitMQProtocol protocol, CancellationToken token = default)
        {
            return protocol.SendCloseOk(1, 0, 10, 51, token);
        }
    }
}
