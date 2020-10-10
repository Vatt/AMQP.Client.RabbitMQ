using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Connections;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AMQP.Client.RabbitMQ.Network
{
    public class NetworkConnectionFactory : ConnectionFactory
    {
        private ILogger _logger;
        public NetworkConnectionFactory(ILogger logger)
        {
            _logger = logger;
        }
        public override async ValueTask<Connection> ConnectAsync(EndPoint? endPoint, IConnectionProperties? options = null,
                                                                 CancellationToken cancellationToken = new CancellationToken())
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endPoint);
            var ns = new NetworkStream(socket);
            return Connection.FromStream(ns, localEndPoint: socket.LocalEndPoint, remoteEndPoint: socket.RemoteEndPoint);
        }
    }
}