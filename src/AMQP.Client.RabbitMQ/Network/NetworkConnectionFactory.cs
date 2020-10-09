using System.Diagnostics;
using System.Net;
using System.Net.Connections;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Network.Internal.Pool;

namespace AMQP.Client.RabbitMQ.Network
{
    public class NetworkConnectionFactory : ConnectionFactory
    {
        private RabbitMQMemoryPool _pool;
        public NetworkConnectionFactory()
        {
            _pool = new RabbitMQMemoryPool();
        }
        public override async ValueTask<Connection> ConnectAsync(EndPoint? endPoint, IConnectionProperties? options = null,
            CancellationToken cancellationToken = new CancellationToken())
        {

            Debug.Assert(endPoint != null, nameof(endPoint) + " != null");
            //var pipe = DuplexPipe.CreateConnectionPair(_pool);
            var connection = new RabbitMQNetworkConnection(endPoint, _pool);
            await connection.ConnectAsync();
            return connection;
            
            // var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            // await socket.ConnectAsync(endPoint);
            // var ns = new NetworkStream(socket);
            // return Connection.FromStream(ns, localEndPoint: socket.LocalEndPoint, remoteEndPoint: socket.RemoteEndPoint);
        }
    }
}