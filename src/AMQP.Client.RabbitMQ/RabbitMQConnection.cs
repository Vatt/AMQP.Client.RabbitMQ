using Microsoft.AspNetCore.Connections;
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
using System.Collections.Concurrent;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System.Runtime.CompilerServices;
using AMQP.Client.RabbitMQ.Protocol.Methods.Common;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection
    {
        private static readonly Bedrock.Framework.Client _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider())
                                                                    .UseSockets()
                                                                    .Build();
        private static ushort _channelId = 0; //Interlocked?
        private readonly ConcurrentDictionary<ushort, RabbitMQDefaultChannel> _channels;
        private RabbitMQChannelZero Channel0;
        private readonly object _lockObj = new object();
        private CancellationToken _connectionClosed;
        private CancellationTokenSource _cts;
        private TaskCompletionSource<bool> _endReading;
        private ConnectionContext _context;
        public readonly EndPoint RemoteEndPoint;
        private Task _readingTask;
        private Task _connectionClosedTask;
        private RabbitMQProtocol _protocol;        
        public RabbitMQServerInfo ServerInfo => Channel0.ServerInfo;
        public RabbitMQMainInfo MainInfo => Channel0.MainInfo;
        public RabbitMQClientInfo ClientInfo => Channel0.ClientInfo;

        private readonly RabbitMQConnectionBuilder _builder;
        public RabbitMQConnection(RabbitMQConnectionBuilder builder)
        {
            RemoteEndPoint = builder?.Endpoint;
            _builder = builder;
            _cts = new CancellationTokenSource();
            _endReading = new TaskCompletionSource<bool>();
            _connectionClosed = _cts.Token;
            _channels = new ConcurrentDictionary<ushort, RabbitMQDefaultChannel>();                       
        }

        public async Task StartAsync()
        {
            _context = await _client.ConnectAsync(RemoteEndPoint, _cts.Token).ConfigureAwait(false);
            _protocol = new RabbitMQProtocol(_context);
            Channel0 = new RabbitMQChannelZero(_builder, _protocol);
            _readingTask = StartReading();
            bool openned = await Channel0.TryOpenChannelAsync().ConfigureAwait(false);
            if (!openned)
            {
                _context.Abort();
                _cts.Cancel();
                return;
            }
            _connectionClosedTask =  WaitClose();
        }
        private async Task WaitClose()
        {
            var info = await Channel0.WaitClosing().ConfigureAwait(false);
            Console.WriteLine($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
            _endReading.SetResult(false);
            _context.Abort();
            _cts.Cancel();
        }
        private async Task StartReading()
        {
            var headerReader = new FrameHeaderReader();
            try
            {
                while (true)
                {
                    var result = await _protocol.Reader.ReadAsync(headerReader, _cts.Token).ConfigureAwait(false);
                    _protocol.Reader.Advance();
                    if (result.IsCompleted)
                    {
                        break;
                    }
                    var header = result.Message;
                    switch (header.FrameType)
                    {
                        case 1:
                            {
                                if (header.Channel == 0)
                                {
                                    await Channel0.HandleAsync(header).ConfigureAwait(false);
                                    break;
                                }
                                await ProcessChannels(header).ConfigureAwait(false);
                                break;
                            }
                        case 2:
                            {
                                await ProcessChannels(header).ConfigureAwait(false);
                                break;
                            }
                        case 8:
                            {
                                await _protocol.Reader.ReadAsync(new NoPayloadReader()).ConfigureAwait(false);
                                _protocol.Reader.Advance();
                                //await _protocol.Writer.WriteAsync(new HeartbeatWriter(), false).ConfigureAwait(false);
                                break;
                            }


                        default: throw new Exception($"Frame type missmatch:{header.FrameType}, {header.Channel}, {header.PaylodaSize}");
                    }
                }
            }
            catch (Exception e)
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
            var id = ++_channelId;
            if (id > Channel0.MainInfo.ChannelMax)
            {
                return default;
            }
            var channel = new RabbitMQDefaultChannel(_protocol, id, MainInfo, RemoveChannelPrivate);
            _channels[id] = channel;
            var openned = await channel.TryOpenChannelAsync().ConfigureAwait(false);
            if (!openned)
            {
                if (!_channels.TryRemove(id, out RabbitMQDefaultChannel _))
                {
                    //TODO: сделать что нибудь
                }
                return default;
            }
            return channel;
        }
        private void RemoveChannelPrivate(ushort id)
        {
            if (!_channels.TryRemove(id, out RabbitMQDefaultChannel channel))
            {
                //TODO: сделать что нибудь
            }
            if (channel != null && channel.IsOpen)
            {
                //TODO: сделать что нибудь
            }
        }

        public async ValueTask CloseConnection()
        {
            await Channel0.CloseChannelAsync("Connection closed gracefully").ConfigureAwait(false);

        }
        public Task WaitEndReading()
        {
            return _endReading.Task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask ProcessChannels(FrameHeader header)
        {
            if (!_channels.TryGetValue(header.Channel, out RabbitMQDefaultChannel channel))
            {
                throw new Exception($"{nameof(RabbitMQConnection)}: channel-id({header.Channel}) missmatch");
            }            
            return channel.HandleFrameHeaderAsync(header);
        }
    }
}
