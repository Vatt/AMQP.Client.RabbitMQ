using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Exceptions;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    sealed class RabbitMQSession : IConnectionHandler, IAsyncDisposable
    {
        private RabbitMQListener _listener;
        private ChannelHandler _channelHandler;
        private CancellationTokenSource _cts;
        private ConnectionContext _ctx;

        private Timer _heartbeat;
        private Task _readingTask;
        private Task _watchTask;
        private TaskCompletionSource<bool> _connectionCloseOkSrc;
        private TaskCompletionSource<bool> _connectionOpenOk;
        private TaskCompletionSource<CloseInfo> _connectionClosedSrc;
       
        public readonly ILogger Logger;
        public readonly ConnectionOptions Options;
        public RabbitMQProtocolWriter Writer { get; private set; }
        public RabbitMQProtocolReader Reader { get; private set; }

        internal readonly ConcurrentDictionary<ushort, ChannelData> Channels;

        public ServerConf ServerOptions;
        public readonly Guid ConnectionId;
        public RabbitMQSession(RabbitMQConnectionFactoryBuilder builder, ConcurrentDictionary<ushort, ChannelData> channels)
        {
            Channels = channels;
            Options = builder.Options;
            Logger = builder.Logger;
            ConnectionId = Guid.NewGuid();
            Channels = channels;
        }
        public Task<RabbitMQChannel> OpenChannel()
        {
            return _channelHandler.OpenChannel();
        }
        public async ValueTask DisposeAsync()
        {
            _heartbeat?.Dispose();
            await Writer.DisposeAsync();
            await Reader.DisposeAsync();
            await _ctx.DisposeAsync();
            _connectionCloseOkSrc.SetCanceled();
            _connectionOpenOk.SetCanceled();
            _connectionClosedSrc.SetCanceled();
        }

        ValueTask IConnectionHandler.OnCloseAsync(CloseInfo info)
        {
            Logger.LogDebug($"RabbitMQConnection {ConnectionId}: Close received");
            _connectionClosedSrc.SetResult(info);
            return default;
        }

        ValueTask IConnectionHandler.OnCloseOkAsync()
        {
            Logger.LogDebug($"RabbitMQConnection {ConnectionId}: CloseOk received");
            _connectionCloseOkSrc.SetResult(true);
            return default;
        }

        ValueTask IConnectionHandler.OnHeartbeatAsync()
        {
            return default;
        }

        ValueTask IConnectionHandler.OnOpenOkAsync()
        {
            Logger.LogDebug($"RabbitMQConnection {ConnectionId}: OpenOk received");
            _heartbeat = new Timer(Heartbeat, null, 0, Options.TuneOptions.Heartbeat);
            _connectionOpenOk.SetResult(true);
            return default;
        }

        async ValueTask IConnectionHandler.OnStartAsync(ServerConf conf)
        {
            Logger.LogDebug($"RabbitMQConnection {ConnectionId}: Start received");
            ServerOptions = conf;
            await Writer.SendStartOkAsync(Options.ClientOptions, Options.ConnOptions).ConfigureAwait(false);
        }

        async ValueTask IConnectionHandler.OnTuneAsync(TuneConf conf)
        {
            Logger.LogDebug($"RabbitMQConnection {ConnectionId}: Tune received");
            if (Options.TuneOptions.ChannelMax > conf.ChannelMax || Options.TuneOptions.ChannelMax == 0 && conf.ChannelMax != 0)
            {
                Options.TuneOptions.ChannelMax = conf.ChannelMax;
            }
            if (Options.TuneOptions.FrameMax > conf.FrameMax)
            {
                Options.TuneOptions.FrameMax = conf.FrameMax;
            }
            await Writer.SendTuneOkAsync(Options.TuneOptions).ConfigureAwait(false);
            await Writer.SendOpenAsync(Options.ConnOptions.VHost).ConfigureAwait(false);
        }
        private void Heartbeat(object state)
        {
            _ = HeartbeatAsync();
        }

        private async ValueTask HeartbeatAsync()
        {
            try
            {
                await Writer.SendHeartbeat().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                //TODO logger
            }
        }

        public async ValueTask Connect()
        {
            var _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider()) //.UseClientTls()
                                .UseSockets()
                                .Build();
            _connectionOpenOk = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _cts = new CancellationTokenSource();
            _ctx = await _client.ConnectAsync(Options.Endpoint, _cts.Token).ConfigureAwait(false);
            Writer = new RabbitMQProtocolWriter(_ctx);
            await Writer.SendProtocol(_cts.Token).ConfigureAwait(false);

            _channelHandler = new ChannelHandler(this);
            _connectionClosedSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            _connectionCloseOkSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            StartReadingAsync(new RabbitMQProtocolReader(_ctx));
            _watchTask = WatchAsync();
            await _connectionOpenOk.Task.ConfigureAwait(false);
        }
        private void StartReadingAsync(RabbitMQProtocolReader reader)
        {
            _listener = new RabbitMQListener();
            _readingTask = StartReadingInner(reader);
        }
        private async Task StartReadingInner(RabbitMQProtocolReader reader)
        {
            try
            {
                await _listener.StartAsync(reader, this, _channelHandler, Logger, _cts.Token).ConfigureAwait(false);
            }
            catch (RabbitMQException e)
            {
                _connectionClosedSrc.SetException(e);

            }
            catch (IOException e)
            {
                _connectionClosedSrc.SetException(e);

            }
        }
        private async Task WatchAsync()
        {
            try
            {
                var info = await _connectionClosedSrc.Task.ConfigureAwait(false);
                Logger.LogInformation($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
               // _channelHandler.Stop();
            }
            catch (SocketException e)
            {
                //_channelHandler.Stop(e);
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
            }
            catch (IOException e)
            {
               // _channelHandler.Stop(e);
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
            }
            catch (RabbitMQException e)
            {
                //_channelHandler.Stop(e);
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
            }
            catch (Exception e)
            {
                //_channelHandler.Stop(e);
                Logger.LogError(e.Message);
                Logger.LogError(e.StackTrace);
            }
            finally
            {
                _listener.Stop();
                _ctx.Abort();
                _heartbeat?.Dispose();
            }
        }
    }
}
