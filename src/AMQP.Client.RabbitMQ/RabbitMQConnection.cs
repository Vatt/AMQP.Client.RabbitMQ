using Microsoft.AspNetCore.Connections;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Bedrock.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Channel;
using System.Diagnostics;
using AMQP.Client.RabbitMQ.Protocol.Common;

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
        private TaskCompletionSource<bool> _endReading;
        private ConnectionContext _context;
        public readonly EndPoint RemoteEndPoint;
        private Task _readingTask;
        private RabbitMQProtocol _protocol;
        public RabbitMQServerInfo ServerInfo => Channel0.ServerInfo;
        public RabbitMQMainInfo MainInfo => Channel0.MainInfo;
        public RabbitMQClientInfo ClientInfo => Channel0.ClientInfo;

        private RabbitMQChannelZero Channel0;
        private RabbitMQChannelHandler _channels;
        private readonly RabbitMQConnectionBuilder _builder;
        public RabbitMQConnection(RabbitMQConnectionBuilder builder)
        {
            RemoteEndPoint = builder?.Endpoint;
            _builder = builder;
            _cts = new CancellationTokenSource();
            _endReading = new TaskCompletionSource<bool>();
        }
        
        public async Task StartAsync()
        { 
            _context = await _client.ConnectAsync(RemoteEndPoint, _cts.Token);
            _connectionClosed = _cts.Token;
            _protocol = new RabbitMQProtocol(_context);
            Channel0 = new RabbitMQChannelZero(_builder, _protocol);
            _channels = new RabbitMQChannelHandler(_protocol, Channel0);
            _readingTask = StartReading();
            bool openned = await Channel0.TryOpenChannelAsync();
            if(!openned)
            {
                _context.Abort();
                _cts.Cancel();
                return;
            }
            

        }
        private async Task StartReading()
        {
            var headerReader = new FrameHeaderReader();
            try
            {
                while (true)
                {                    
                    var result = await _protocol.Reader.ReadAsync(headerReader, _cts.Token);
                    _protocol.Reader.Advance();
                    if (result.IsCompleted)
                    {
                        break;
                    }
                    var header = result.Message;
                    Debug.WriteLine($"{header.FrameType} {header.Channel} {header.PaylodaSize}");
                    switch (header.FrameType)
                    {
                        case 1:
                            {
                                if (header.Channel == 0)
                                {
                                    await Channel0.HandleAsync(header);
                                    break;
                                }
                                await _channels.HandleFrameAsync(header);
                                break;
                            }
                       case 2:
                            {
                                await _channels.HandleFrameAsync(header);
                                break;
                            }
                       case 8:
                            {
                                await _protocol.Reader.ReadAsync(new NoPayloadReader());
                                _protocol.Reader.Advance();
                                await _protocol.Writer.WriteAsync(new HeartbeatWriter(),false);
                                break;
                            }
                            
                            
                        default: throw new Exception($"Frame type missmatch:{header.FrameType}, {header.Channel}, {header.PaylodaSize}");
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                _context.Abort();
                _cts.Cancel();
                _endReading.SetResult(false);
            }
  
        }
        public async ValueTask<IRabbitMQDefaultChannel> CreateChannel()
        {
            return await _channels.CreateChannel();
        }

        public async ValueTask CloseConnectionAsync()
        {
            _context.Abort();
            _cts.Cancel();
            _endReading.SetResult(false);

        }
        public async Task WaitEndReading()
        {
            await _endReading.Task;
        }
    }
}
