using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Connections;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Network.Internal.Pipe;
using AMQP.Client.RabbitMQ.Network.Internal.Pool;


namespace AMQP.Client.RabbitMQ.Network
{
    internal class RabbitMQNetworkConnection : Connection
    {
        private RabbitMQMemoryPool _pool;
        private Socket _socket;
        private EndPoint _endpoint;

        private SocketSender _sender;
        private SocketReceiver _receiver;

        public IDuplexPipe _transport;
        private IDuplexPipe _application;


        public RabbitMQNetworkConnection(EndPoint endpoint, RabbitMQMemoryPool pool)
        {
            _pool = pool;
            _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            _endpoint = endpoint;

            _sender = new SocketSender(_socket, PipeScheduler.ThreadPool);
            _receiver = new SocketReceiver(_socket, PipeScheduler.ThreadPool);
        }

        public async ValueTask ConnectAsync()
        {
            var pair = DuplexPipe.CreateConnectionPair(_pool);
            _transport = pair.Transport;
            _application = pair.Application;

            await _socket.ConnectAsync(_endpoint).ConfigureAwait(false);

            _ = ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            var sendsTask = Task.Run(ProcessSends);
            var receivesTask = Task.Run(ProcessReceives);
            if (await Task.WhenAny(receivesTask, sendsTask) == sendsTask)
            {
                _socket.Dispose();
            }

            await receivesTask;
        }

        private async Task ProcessReceives()
        {
            while (true)
            {
                var buffer = _application.Output.GetMemory();
                var bytesReceived = await _receiver.ReceiveAsync(buffer);
                _application.Output.Advance(bytesReceived);
                await _application.Output.FlushAsync();
            }
        }

        private async Task ProcessSends()
        {
            while (true)
            {
                var result = await _application.Input.ReadAsync().ConfigureAwait(false);
                var buffer = result.Buffer;

                await _sender.SendAsync(buffer);
                _application.Input.AdvanceTo(buffer.End);

                if (result.IsCompleted)
                {
                    break;
                }
            }
        }

        protected override IDuplexPipe CreatePipe()
        {
            return _transport;
        }
        protected override ValueTask CloseAsyncCore(ConnectionCloseMethod method, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override IConnectionProperties ConnectionProperties { get; }
        public override EndPoint? LocalEndPoint => _socket.LocalEndPoint;
        public override EndPoint? RemoteEndPoint => _socket.RemoteEndPoint;
    }
}