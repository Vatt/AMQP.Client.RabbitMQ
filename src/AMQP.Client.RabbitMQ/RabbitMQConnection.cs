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
using AMQP.Client.RabbitMQ.Channel;
using System.Diagnostics;
using System.Net.Sockets;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection
    {
        private static readonly Bedrock.Framework.Client _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider())
                                                                    .UseSockets()
                                                                    .Build();
        
        private readonly object _lockObj = new object();
        private CancellationToken _connectionClosed;
        private CancellationTokenSource _cts;
        private ConnectionContext _context;
        public readonly EndPoint RemoteEndPoint;
        public IDuplexPipe Transport => _context.Transport;
        private Task _readingTask;
        private RabbitMQProtocol _protocol;
        private Heartbeat _heartbeat;
        public RabbitMQServerInfo ServerInfo => Channel0.ServerInfo;
        public RabbitMQMainInfo MainInfo => Channel0.MainInfo;
        public RabbitMQClientInfo ClientInfo => Channel0.ClientInfo;

        private RabbitMQChannel0 Channel0;
        private RabbitMQChannelManager _channels;
        private readonly RabbitMQConnectionBuilder _builder;
        public RabbitMQConnection(RabbitMQConnectionBuilder builder)
        {
            RemoteEndPoint = builder?.Endpoint;
            _builder = builder;
            _cts = new CancellationTokenSource();
        }
        
        public async Task StartAsync()
        {
            _context = await _client.ConnectAsync(RemoteEndPoint, _cts.Token);
            _connectionClosed = _cts.Token;
            _protocol = new RabbitMQProtocol(_context);
            Channel0 = new RabbitMQChannel0(_builder, _protocol);

            _readingTask = StartReading();
            bool openned = await Channel0.TryOpenChannelAsync();
            if(!openned)
            {
                _context.Abort();
                _cts.Cancel();
                return;
            }
            _heartbeat = new Heartbeat(Transport.Output, new TimeSpan(MainInfo.Heartbeat), _cts.Token);
            _channels = new RabbitMQChannelManager(_protocol, MainInfo.ChannelMax);

        }
        private async Task StartReading()
        {

            var headerReader = new FrameHeaderReader();
            while (true)
            {
                var result = await _protocol.Reader.ReadAsync(headerReader, _cts.Token);
                _protocol.Reader.Advance();
                if (result.IsCompleted)
                {
                    break;
                }
                var header = result.Message;
                Debug.WriteLine($"{header.FrameType} {header.Chanell} {header.PaylodaSize}");
                switch (header.FrameType)
                {
                    case 1:
                        {
                            if (header.Chanell == 0)
                            {
                                await Channel0.HandleAsync(header);
                                break;
                            }
                            await _channels.HandleFrameAsync(header);
                            break;
                        }
                    /*case 59:
                        {

                            break;
                        }
                    case 8:
                        {
                            break;
                        }
                        */
                    default: throw new Exception($"Frame type missmatch:{header.FrameType}");
                }
            }
  
        }
        public async ValueTask<IRabbitMQChannel> CreateChannel()
        {
            return await _channels.CreateChannel();
        }
        private void StartMethodSuccess()
        {            
            _heartbeat.StartAsync();
        }

        public async ValueTask CloseConnectionAsync()
        {
            _context.Abort();
            _cts.Cancel();        
            
        }
        public void WaitEndReading()
        {
            Task.WaitAll(_readingTask);
        }
    }
}
