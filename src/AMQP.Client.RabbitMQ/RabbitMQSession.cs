using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Exceptions;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    sealed partial class RabbitMQSession : IConnectionHandler, IAsyncDisposable
    {
        private RabbitMQListener _listener;
        //private ChannelHandler _channelHandler;
        private CancellationTokenSource _cts;
        private ConnectionContext _ctx;

        private Timer _heartbeat;
        private TaskCompletionSource<bool> _connectionCloseOkSrc;
        private TaskCompletionSource<bool> _connectionOpenOk;
        internal TaskCompletionSource ConnectionGlobalLock;
        internal readonly TaskCompletionSource<CloseInfo> ConnectionClosedSrc;

        public readonly ILogger Logger;
        public readonly ConnectionOptions Options;
        public RabbitMQProtocolWriter Writer { get; private set; }
        public RabbitMQProtocolReader Reader { get; private set; }

        internal readonly ConcurrentDictionary<ushort, RabbitMQChannel> Channels;
        public ServerConf ServerOptions;
        public readonly Guid ConnectionId;
        public RabbitMQSession(RabbitMQConnectionFactoryBuilder builder, ConcurrentDictionary<ushort, RabbitMQChannel> channels, TaskCompletionSource<CloseInfo> closeSrc, TaskCompletionSource connectionLock)
        {
            if (channels == null || closeSrc == null)
            {
                throw new NullReferenceException(nameof(channels));
            }
            Channels = channels;
            Options = builder.Options;
            Logger = builder.Logger;
            ConnectionId = Guid.NewGuid();
            Channels = channels;
            ConnectionClosedSrc = closeSrc;
            ConnectionGlobalLock = connectionLock;
        }
        public async ValueTask DisposeAsync()
        {
            _heartbeat?.Dispose();
            await Writer.DisposeAsync().ConfigureAwait(false);
            await Reader.DisposeAsync().ConfigureAwait(false);
            //_ctx.Abort();
            _cts.Cancel();
            await _ctx.DisposeAsync().ConfigureAwait(false);
            _listener.Stop();
            CancelTcs();
            if (!_connectionCloseOkSrc.Task.IsCompleted && !_connectionCloseOkSrc.Task.IsCanceled)
            {
                _connectionCloseOkSrc.SetCanceled();
            }
            if (!_connectionOpenOk.Task.IsCompleted && !_connectionOpenOk.Task.IsCanceled)
            {
                _connectionOpenOk.SetCanceled();
            }
            if (!ConnectionClosedSrc.Task.IsCompleted && !ConnectionClosedSrc.Task.IsCanceled)
            {
                ConnectionClosedSrc.SetCanceled();
            }
        }

        ValueTask IConnectionHandler.OnCloseAsync(CloseInfo info)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)} {ConnectionId}: Close received");
            ConnectionClosedSrc.SetResult(info);
            return default;
        }

        ValueTask IConnectionHandler.OnCloseOkAsync()
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)} {ConnectionId}: CloseOk received");
            _connectionCloseOkSrc.SetResult(true);
            return default;
        }

        ValueTask IConnectionHandler.OnHeartbeatAsync()
        {
            return default;
        }

        ValueTask IConnectionHandler.OnOpenOkAsync()
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)} {ConnectionId}: OpenOk received");
            _heartbeat = new Timer(Heartbeat, null, 0, Options.TuneOptions.Heartbeat);
            _connectionOpenOk.SetResult(true);
            return default;
        }

        async ValueTask IConnectionHandler.OnStartAsync(ServerConf conf)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)} {ConnectionId}: Start received");
            ServerOptions = conf;
            await Writer.SendStartOkAsync(Options.ClientOptions, Options.ConnOptions).ConfigureAwait(false);
        }

        async ValueTask IConnectionHandler.OnTuneAsync(TuneConf conf)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)} {ConnectionId}: Tune received");
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
            _connectionOpenOk = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _cts = new CancellationTokenSource();
            _ctx = await TryConnect();
            Writer = new RabbitMQProtocolWriter(_ctx);
            Reader = new RabbitMQProtocolReader(_ctx);
            await Writer.SendProtocol(_cts.Token).ConfigureAwait(false);            
            _connectionCloseOkSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            StartReadingAsync(Reader);
            await _connectionOpenOk.Task.ConfigureAwait(false);
        }
        private async ValueTask<ConnectionContext> TryConnect()
        {
            var client = new ClientBuilder(new ServiceCollection().BuildServiceProvider()) //.UseClientTls()
                            .UseSockets()
                            .Build();
            for (var i = 0; i < Options.ConnectionAttempts; i++)
            {
                try
                {
                    var cts = new CancellationTokenSource(Options.ConnectionTimeout);
                    return await client.ConnectAsync(Options.Endpoint, cts.Token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    continue;
                }
            }
            return default;
        }
        public async ValueTask ConnectWithRecovery()
        {
            await Connect().ConfigureAwait(false);
            try
            {
                await Recovery().ConfigureAwait(false);
            }catch(Exception e)
            {
                Debugger.Break();
            }
            
        }
        private void StartReadingAsync(RabbitMQProtocolReader reader)
        {
            _listener = new RabbitMQListener();
            _ = StartReadingInner(reader);
        }
        private async Task StartReadingInner(RabbitMQProtocolReader reader)
        {
            try
            {
                await _listener.StartAsync(reader, this, this, _cts.Token).ConfigureAwait(false);
            }
            catch (RabbitMQException e)
            {
                ConnectionClosedSrc.SetException(e);
            }
            catch (IOException e)
            {
                ConnectionClosedSrc.SetException(e);
            }
            catch (SocketException e)
            {
                ConnectionClosedSrc.SetException(e);

            }
            catch (Exception e)
            {
                ConnectionClosedSrc.SetException(e);
            }
        }
        public async Task CloseAsync(string reason = null)
        {
            var replyText = reason == null ? "Connection closed gracefully" : reason;
            var info = new CloseInfo(RabbitMQConstants.Success, replyText, 0, 0);
            await Writer.SendConnectionCloseAsync(info).ConfigureAwait(false);
            await _connectionCloseOkSrc.Task.ConfigureAwait(false);
            ConnectionClosedSrc.SetResult(info);
        }
        private async ValueTask Recovery()
        {
            foreach (var channelPair in Channels)
            {
                var channel = channelPair.Value;
                await channel.WriterSemaphore.WaitAsync();
                channel.Session = this;

                _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                await Writer.SendChannelOpenAsync(channelPair.Key).ConfigureAwait(false);
                await _openSrc.Task.ConfigureAwait(false);


                foreach (var exchange in channelPair.Value.Exchanges.Values)
                {
                    if (exchange.NoWait)
                    {
                        await Writer.SendExchangeDeclareAsync(channelPair.Key, exchange).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        channel.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                        await Writer.SendExchangeDeclareAsync(channelPair.Key, exchange).ConfigureAwait(false);
                        await channel.CommonTcs.Task.ConfigureAwait(false);
                    }

                }
                foreach (var queue in channelPair.Value.Queues.Values)
                {
                    if (queue.NoWait)
                    {
                        await Writer.SendQueueDeclareAsync(channelPair.Key, queue).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        channel.QueueTcs = new TaskCompletionSource<QueueDeclareOk>(TaskCreationOptions.RunContinuationsAsynchronously);
                        await Writer.SendQueueDeclareAsync(channelPair.Key, queue).ConfigureAwait(false);
                        var declare = await channel.QueueTcs.Task.ConfigureAwait(false);
                    }

                }
                foreach (var bind in channelPair.Value.Binds.Values)
                {
                    if (bind.NoWait)
                    {
                        await Writer.SendQueueBindAsync(channelPair.Key, bind).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        channel.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                        await Writer.SendQueueBindAsync(channelPair.Key, bind).ConfigureAwait(false);
                        await channel.CommonTcs.Task.ConfigureAwait(false);
                    }
                }
                foreach (var consumer in channelPair.Value.Consumers.Values)
                {
                    if (consumer.Conf.NoWait)
                    {
                        await Writer.SendBasicConsumeAsync(channelPair.Key, consumer.Conf).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        channel.ConsumeTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                        await Writer.SendBasicConsumeAsync(channelPair.Key, consumer.Conf).ConfigureAwait(false);
                        var tag = await channel.ConsumeTcs.Task.ConfigureAwait(false);
                        if (!tag.Equals(consumer.Conf.ConsumerTag))
                        {
                            RabbitMQExceptionHelper.ThrowIfConsumeOkTagMissmatch(consumer.Conf.ConsumerTag, tag);
                        }
                    }
                }
                channel.IsClosed = false;
                channel.WriterSemaphore.Release();
            }
        }
    }
}
