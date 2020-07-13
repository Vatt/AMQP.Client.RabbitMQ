using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection : IConnectionHandler
    {
        public readonly ConnectionOptions Options;
        private ChannelHandler _channelHandler;
        private TaskCompletionSource<bool> _closeSrc;
        private TaskCompletionSource<bool> _openOk;
        private TaskCompletionSource<CloseInfo> _connectionCloseSrc;
        private CancellationTokenSource _cts;
        private ConnectionContext _ctx;
        private Timer _heartbeat;
        private RabbitMQListener _listener;
        private Task _readingTask;
        private Task _watchTask;
        private RabbitMQProtocolWriter _writer;
        public EventHandler ConnectionBlocked;
        public EventHandler ConnectionUnblocked;
        public EventHandler ConnenctionClosed;
        public ServerConf ServerOptions;

        internal RabbitMQConnection(RabbitMQConnectionFactoryBuilder builder)
        {
            Options = builder.Options;
        }

        public ValueTask OnCloseAsync(CloseInfo info)
        {
            _connectionCloseSrc.SetResult(info);
            return default;
        }

        public ValueTask OnCloseOkAsync()
        {
            _closeSrc.SetResult(true);
            return default;
        }

        public ValueTask OnHeartbeatAsync()
        {
            return default;
        }

        public ValueTask OnOpenOkAsync()
        {
            _heartbeat = new Timer(Heartbeat, null, 0, Options.TuneOptions.Heartbeat);
            _openOk.SetResult(true);
            return default;
        }

        public async ValueTask OnStartAsync(ServerConf conf)
        {
            ServerOptions = conf;
            await _writer.SendStartOkAsync(Options.ClientOptions, Options.ConnOptions).ConfigureAwait(false);
        }

        public async ValueTask OnTuneAsync(TuneConf conf)
        {
            if (Options.TuneOptions.ChannelMax > conf.ChannelMax || Options.TuneOptions.ChannelMax == 0 && conf.ChannelMax != 0)
            {
                Options.TuneOptions.ChannelMax = conf.ChannelMax;
            }
            if (Options.TuneOptions.FrameMax > conf.FrameMax)
            {
                Options.TuneOptions.FrameMax = conf.FrameMax;
            }
            await _writer.SendTuneOkAsync(Options.TuneOptions).ConfigureAwait(false);
            await _writer.SendOpenAsync(Options.ConnOptions.VHost).ConfigureAwait(false);
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
                await _listener.StartAsync(reader, this, _channelHandler, _cts.Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _connectionCloseSrc.SetException(e);
            }
        }

        public async Task StartAsync()
        {
            var _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider()) //.UseClientTls()
                .UseSockets()
                .Build();
            _openOk = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _cts = new CancellationTokenSource();
            _ctx = await _client.ConnectAsync(Options.Endpoint, _cts.Token).ConfigureAwait(false);
            _writer = new RabbitMQProtocolWriter(_ctx);
            await _writer.SendProtocol(_cts.Token).ConfigureAwait(false);


            _channelHandler = new ChannelHandler(_writer, Options);
            _connectionCloseSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            _closeSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            StartReadingAsync(new RabbitMQProtocolReader(_ctx));
            _watchTask = WatchAsync();
        }
        private async Task RestartAsync()
        {
            var _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider()) //.UseClientTls()
                            .UseSockets()
                            .Build();
            _cts = new CancellationTokenSource();
            _ctx = await _client.ConnectAsync(Options.Endpoint, _cts.Token).ConfigureAwait(false);
            _openOk = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _writer = new RabbitMQProtocolWriter(_ctx);
            await _writer.SendProtocol(_cts.Token).ConfigureAwait(false);

            _connectionCloseSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            _closeSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            StartReadingAsync(new RabbitMQProtocolReader(_ctx));
            _watchTask = WatchAsync();
            await _openOk.Task.ConfigureAwait(false);
            await _channelHandler.Recovery(_writer);
        }
        public async Task CloseAsync(string reason = null)
        {
            var replyText = reason == null ? "Connection closed gracefully" : reason;
            var info = new CloseInfo(Constants.Success, replyText, 0, 0);
            await _writer.SendConnectionCloseAsync(info).ConfigureAwait(false);
            await _closeSrc.Task.ConfigureAwait(false);
            _connectionCloseSrc.SetResult(info);
            _cts.Cancel();
        }

        private async Task WatchAsync()
        {
            try
            {
                var info = await _connectionCloseSrc.Task.ConfigureAwait(false);
                Console.WriteLine($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
                //_cts.Cancel();
                //_ctx.Abort();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Connection closed with exceptions: {e.Message}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                //_cts.Cancel();
                _ctx.Abort();
                _heartbeat?.Dispose();
//                ChannelHandlerUnlockSemaphore();

            }
            ChannelHandlerLock();
            //await ChannelHandlerLockSemaphore().ConfigureAwait(false);
            try
            {
                await RestartAsync();
            }
            finally
            {
                ChannelHandlerUnlock();
                //ChannelHandlerUnlockSemaphore();
            }
            ChannelHandlerUnlock();
            //ChannelHandlerUnlockSemaphore();


        }
        private async ValueTask ChannelHandlerLockSemaphore()
        {
            foreach (var data in _channelHandler.Channels.Values)
            {
                await data.WriterSemaphore.WaitAsync().ConfigureAwait(false);                
            }
        }
        private void ChannelHandlerUnlockSemaphore()
        {
            foreach (var data in _channelHandler.Channels.Values)
            {
                data.WriterSemaphore.Release();
               
            }
        }
        private void ChannelHandlerLock()
        {
            foreach(var data in _channelHandler.Channels.Values)
            {
                //await data.WriterSemaphore.WaitAsync();
                data.waitTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }
        private void ChannelHandlerUnlock()
        {
            foreach (var data in _channelHandler.Channels.Values)
            {
                //data.WriterSemaphore.Release();
                data.waitTcs.SetResult(false);
            }
        }
        public Task<RabbitMQChannel> OpenChannel()
        {
            return _channelHandler.OpenChannel();
        }

        private void Heartbeat(object state)
        {
            _ = HeartbeatAsync();
        }

        private async ValueTask HeartbeatAsync()
        {
            try
            {
                await _writer.SendHeartbeat().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                //TODO logger
            }
        }
    }
}