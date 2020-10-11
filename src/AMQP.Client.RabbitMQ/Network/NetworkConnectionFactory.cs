using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Connections;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Network
{
    public class NetworkConnectionFactory : ConnectionFactory
    {
        private ILogger _logger;
        public NetworkConnectionFactory(ILogger logger)
        {
            _logger = logger;
        }
        public override async ValueTask<Connection?> ConnectAsync(EndPoint? endPoint, IConnectionProperties? options = null,
                                                                 CancellationToken cancellationToken = new CancellationToken())
        {
            Debug.Assert(endPoint != null, nameof(endPoint) + " != null");
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endPoint, cancellationToken);
            if (!socket.Connected)
            {
                return null;
            }
            var ns = new NetworkStream(socket);
            return Connection.FromStream(ns, localEndPoint: socket.LocalEndPoint, remoteEndPoint: socket.RemoteEndPoint);
        }
    }
}