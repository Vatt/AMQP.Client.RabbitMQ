using Microsoft.AspNetCore.Connections;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Methods;
using Bedrock.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection
    {
        private static readonly Bedrock.Framework.Client _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider())
                                                                    .UseSockets()
                                                                    .Build();
        
        private readonly object _lockObj = new object();
        private readonly CancellationTokenSource _connectionCloseTokenSource = new CancellationTokenSource();
        private readonly CancellationToken ConnectionClosed;
        private ConnectionContext _context;
        public EndPoint RemoteEndPoint => _context.RemoteEndPoint;
        public IDuplexPipe Transport => _context.Transport;
        private RabbitMQReader _reader;
        
        public RabbitMQServerInfo ServerInfo { get; private set; }
        public RabbitMQInfo Info { get; private set; }
        public RabbitMQClientInfo ClientInfo { get; private set; }
        private readonly RabbitMQConnectionInfo _connectionInfo;
        public readonly int Chanell;
        public RabbitMQConnection()
        {
            Chanell = 1;
            ConnectionClosed = _connectionCloseTokenSource.Token;
            ClientInfo = RabbitMQClientInfo.DefaultClientInfo();
            Info = RabbitMQInfo.DefaultConnectionInfo();
            _connectionInfo = new RabbitMQConnectionInfo("gamover", "gam2106", "/");
            
        }
        public async Task StartAsync(IPEndPoint endpoint)
        {
            _context = await _client.ConnectAsync(endpoint, _connectionCloseTokenSource.Token);
            _reader = new RabbitMQReader(Transport.Input);
            StartMethod start = new StartMethod(_reader, Transport.Output, Info, _connectionInfo, ClientInfo , ServerInfoReceived, StartMethodSuccess);
            await start.RunAsync();
            await _reader.StartAsync();
        }
        private void ServerInfoReceived(RabbitMQServerInfo info)
        {
            ServerInfo = info;
        }
        /*Сделать чтонибудь*/
        private void StartMethodSuccess()
        {
            
        }

        public void CloseConnection()
        {
            lock (_lockObj)
            {
                _connectionCloseTokenSource.Cancel();
                _context.Abort();
                _connectionCloseTokenSource.Dispose();
            }

        }
    }
}
