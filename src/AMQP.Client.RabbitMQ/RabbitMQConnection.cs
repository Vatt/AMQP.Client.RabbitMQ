﻿using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using Bedrock.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection : IConnectionHandler
    {
        private Task _readingTask;
        private Task _watchTask;
        private RabbitMQListener _listener;
        private ChannelHandler _channelHandler;
        private RabbitMQProtocolWriter _protocol;
        private CancellationTokenSource _cts;
        private TaskCompletionSource<CloseInfo> _connectionCloseSrc;
        private TaskCompletionSource<bool> _closeSrc;

        public readonly ConnectionOptions Options;
        public ServerConf ServerOptions;

        public RabbitMQConnection(RabbitMQConnectionFactoryBuilder builder)
        {
            Options = builder.Options;
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
                await _listener.StartAsync(reader, this, _channelHandler, _cts.Token);
            }
            catch (Exception e)
            {
                _connectionCloseSrc.SetException(e);
            }
        }
        public async Task StartAsync(/* RabbitMQProtocolReader reader, RabbitMQProtocolWriter writer*/ )
        {

            var _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider()) //.UseClientTls()
                .UseSockets()
                .Build();
            _cts = new CancellationTokenSource();
            var ctx = await _client.ConnectAsync(Options.Endpoint, _cts.Token).ConfigureAwait(false);
            _protocol = new RabbitMQProtocolWriter(ctx);
            await _protocol.SendProtocol(_cts.Token);


            _channelHandler = new ChannelHandler(_protocol);
            _connectionCloseSrc = new TaskCompletionSource<CloseInfo>();
            _closeSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            StartReadingAsync(new RabbitMQProtocolReader(ctx));
            _watchTask = WatchAsync();

        }
        public async Task CloseAsync(string reason = null)
        {
            string replyText = reason == null ? "Connection closed gracefully" : reason;
            var info = new CloseInfo(Constants.Success, replyText, 0, 0);
            await _protocol.SendConnectionCloseAsync(info).ConfigureAwait(false);
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
                _cts.Cancel();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Connection closed with exceptions: {e.Message}");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                _cts.Cancel();
            }
        }
        public ValueTask OnCloseAsync(CloseInfo info)
        {
            _connectionCloseSrc.SetResult(info);
            return default;
        }
        public Task<RabbitMQChannel> OpenChannel()
        {
            return _channelHandler.OpenChannel();
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
            return default;
        }

        public async ValueTask OnStartAsync(ServerConf conf)
        {
            ServerOptions = conf;
            await _protocol.SendStartOkAsync(Options.ClientOptions, Options.ConnOptions).ConfigureAwait(false);
        }

        public async ValueTask OnTuneAsync(TuneConf conf)
        {
            if ((Options.TuneOptions.ChannelMax > conf.ChannelMax) || (Options.TuneOptions.ChannelMax == 0 && conf.ChannelMax != 0))
            {
                Options.TuneOptions.ChannelMax = conf.ChannelMax;
            }
            if (Options.TuneOptions.FrameMax > conf.FrameMax)
            {
                Options.TuneOptions.FrameMax = conf.FrameMax;
            }
            await _protocol.SendTuneOkAsync(Options.TuneOptions).ConfigureAwait(false);
            await _protocol.SendOpenAsync(Options.ConnOptions.VHost).ConfigureAwait(false);
        }
    }
}
