using Microsoft.AspNetCore.Connections;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Methods;
using Bedrock.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Chanell;

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
        public readonly EndPoint RemoteEndPoint;
        public IDuplexPipe Transport => _context.Transport;
        private RabbitMQReader _reader;
        private RabbitMQWriter _writer;
        private Heartbeat _heartbeat;
        private RabbitMQInfo _info;
        public RabbitMQServerInfo ServerInfo { get; private set; }
        public RabbitMQInfo Info => _info;
        public RabbitMQClientInfo ClientInfo { get; private set; }
        
        private readonly RabbitMQConnectionInfo _connectionInfo;
        public readonly RabbitMQChanell Chanell0;
        public RabbitMQConnection(RabbitMQConnectionBuilder builder)
        {
            RemoteEndPoint = builder?.Endpoint;
            _info = builder.Info;
            ClientInfo = builder.ClientInfo;
            _connectionInfo = builder.ConnInfo;
            ConnectionClosed = _connectionCloseTokenSource.Token;
            //Chanell0 = new RabbitMQChanell(0);
        }
        
        public async Task StartAsync()
        {
            _context = await _client.ConnectAsync(RemoteEndPoint, _connectionCloseTokenSource.Token);
            _heartbeat = new Heartbeat(Transport.Output, new TimeSpan(Info.Heartbeat), _connectionCloseTokenSource.Token);
            _reader = new RabbitMQReader(Transport.Input, _connectionCloseTokenSource.Token);
            _writer = new RabbitMQWriter(Transport.Output, _connectionCloseTokenSource.Token);
            _reader.OnServerInfoReaded = ServerInfoReceived;
            _reader.OnInfoReaded = InfoReceived;
            StartMethod start = new StartMethod(_writer);
            await start.RunAsync();
            await _reader.StartReading();
        }
        private async ValueTask ServerInfoReceived(RabbitMQServerInfo info)
        {
            ServerInfo = info;
            _writer.WriteStartOk(ClientInfo,_connectionInfo);            
            await _writer.FlushAsync();
            var test = _writer.Test();
        }
        private ValueTask InfoReceived(RabbitMQInfo info)
        {
            if ( (_info.ChanellMax > info.ChanellMax) || (_info.ChanellMax == 0 && info.ChanellMax != 0) )
            {
                _info.ChanellMax = info.ChanellMax;
            }
            if (_info.FrameMax > info.FrameMax)
            {
                _info.FrameMax = info.FrameMax;
            }
            return default;
        }
        private void StartMethodSuccess()
        {            
            _heartbeat.StartAsync();
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
