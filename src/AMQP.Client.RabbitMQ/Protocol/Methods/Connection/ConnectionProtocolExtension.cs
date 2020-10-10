using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Core;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public static class ConnectionProtocolExtension
    {
        private static ConnectionStartReader _connectionStartReader = new ConnectionStartReader();
        private static ConnectionTuneReader _connectionTuneReader = new ConnectionTuneReader();
        private static ConnectionOpenOkReader _connectionOpenOkReader = new ConnectionOpenOkReader();
        public static ValueTask SendStartOkAsync(this ProtocolWriter protocol, ClientConf clientInfo, ConnectionConf connInfo, CancellationToken token = default)
        {
            return protocol.WriteAsync(ProtocolWriters.ConnectionStartOkWriter , (connInfo, clientInfo), token);
        }
        public static ValueTask SendTuneOkAsync(this ProtocolWriter protocol, TuneConf info, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ConnectionTuneOkWriter(), info, token);
        }
        public static ValueTask SendOpenAsync(this ProtocolWriter protocol, string vhost, CancellationToken token = default)
        {
            return protocol.WriteAsync(new ConnectionOpenWriter(), vhost, token);
        }
        public static ValueTask SendConnectionCloseAsync(this ProtocolWriter protocol, CloseInfo info, CancellationToken token = default)
        {
            return protocol.SendClose(info, token);
        }
        public static ValueTask SendConnectionCloseOkAsync(this ProtocolWriter protocol, CancellationToken token = default)
        {
            return protocol.SendCloseOk(1, 0, 10, 51, token);
        }
        public static ServerConf ReadStart(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_connectionStartReader, input);
        }
        public static ValueTask<ServerConf> ReadStartAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_connectionStartReader, token);
        }
        public static TuneConf ReadTuneMethod(this RabbitMQProtocolReader protocol, in ReadOnlySequence<byte> input)
        {
            return protocol.Read(_connectionTuneReader, input);
        }
        public static ValueTask<TuneConf> ReadTuneMethodAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_connectionTuneReader, token);
        }
        public static bool ReadConnectionOpenOk(this RabbitMQProtocolReader protocol, ReadOnlySequence<byte> input)
        {
            return protocol.Read(_connectionOpenOkReader, input);
        }
        public static ValueTask<bool> ReadConnectionOpenOkAsync(this RabbitMQProtocolReader protocol, CancellationToken token = default)
        {
            return protocol.ReadAsync(_connectionOpenOkReader, token);
        }

    }
}
