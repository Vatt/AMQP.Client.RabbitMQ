using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Connections;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Network.Internal.Pool;
using Microsoft.Extensions.Logging;

namespace AMQP.Client.RabbitMQ.Network
{
    public class NetworkConnectionFactory : ConnectionFactory
    {
        private RabbitMQMemoryPool _pool;
        private ILogger _logger;
        public NetworkConnectionFactory(ILogger logger)
        {
            _pool = new RabbitMQMemoryPool();
            _logger = logger;
        }
        public override async ValueTask<Connection> ConnectAsync(EndPoint? endPoint, IConnectionProperties? options = null,
                                                                 CancellationToken cancellationToken = new CancellationToken())
        {

            Debug.Assert(endPoint != null, nameof(endPoint) + " != null");
            //var pipe = DuplexPipe.CreateConnectionPair(_pool);
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endPoint);
            var connection = new SocketConnection(_logger, socket, _pool, PipeScheduler.ThreadPool);
            connection.Start();
            return connection;
            
            // var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            // await socket.ConnectAsync(endPoint);
            // var ns = new NetworkStream(socket);
            // return Connection.FromStream(ns, localEndPoint: socket.LocalEndPoint, remoteEndPoint: socket.RemoteEndPoint);
        }
    }
}