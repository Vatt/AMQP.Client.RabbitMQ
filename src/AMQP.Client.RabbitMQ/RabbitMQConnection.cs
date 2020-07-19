using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Exceptions;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using Bedrock.Framework;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection
    {
        
        private RabbitMQSession _session;
        //public EventHandler ConnectionBlocked;
        //public EventHandler ConnectionUnblocked;
        //public EventHandler ConnenctionClosed;
        private ConcurrentDictionary<ushort, ChannelData> _channels;
        public ref readonly ConnectionOptions Options => ref _session.Options;
        public ref readonly ServerConf ServerOptions => ref _session.ServerOptions;
        public ref readonly Guid ConnectionId => ref _session.ConnectionId;

        internal RabbitMQConnection(RabbitMQConnectionFactoryBuilder builder)
        {
            _channels = new ConcurrentDictionary<ushort, ChannelData>();
            _session = new RabbitMQSession(builder, _channels);
        }

        //private void StartReadingAsync(RabbitMQProtocolReader reader)
        //{
        //    _listener = new RabbitMQListener();
        //    _readingTask = StartReadingInner(reader);
        //}

        //private async Task StartReadingInner(RabbitMQProtocolReader reader)
        //{
        //    try
        //    {
        //        await _listener.StartAsync(reader, this, _channelHandler, _logger, _cts.Token).ConfigureAwait(false);
        //    }
        //    catch (RabbitMQException e)
        //    {                
        //        _connectionClosedSrc.SetException(e);

        //    }
        //    catch (IOException e)
        //    {
        //        _connectionClosedSrc.SetException(e);

        //    }
        //}

        public async Task StartAsync()
        {
            //    var _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider()) //.UseClientTls()
            //        .UseSockets()
            //        .Build();
            //    _connectionOpenOk = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            //    _cts = new CancellationTokenSource();
            //    _ctx = await _client.ConnectAsync(Options.Endpoint, _cts.Token).ConfigureAwait(false);
            //    _writer = new RabbitMQProtocolWriter(_ctx);
            //    await _writer.SendProtocol(_cts.Token).ConfigureAwait(false);


            //    _channelHandler = new ChannelHandler(_writer, Options, _logger);
            //    _connectionClosedSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            //    _connectionCloseOkSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            //    StartReadingAsync(new RabbitMQProtocolReader(_ctx));
            //    _watchTask = WatchAsync();
            //    await _connectionOpenOk.Task.ConfigureAwait(false);
            await _session.Connect();
        }
        //private async Task ReconnectAsync()
        //{
        //    var _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider())//.UseClientTls()
        //                    .UseSockets()
        //                    .Build();
        //    _cts = new CancellationTokenSource();
        //    _ctx = await _client.ConnectAsync(Options.Endpoint, _cts.Token).ConfigureAwait(false);
        //    _connectionOpenOk = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        //    _writer = new RabbitMQProtocolWriter(_ctx);
        //    await _writer.SendProtocol(_cts.Token).ConfigureAwait(false);

        //    _connectionClosedSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        //    _connectionCloseOkSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        //    StartReadingAsync(new RabbitMQProtocolReader(_ctx));
        //    _watchTask = WatchAsync();
        //    await _connectionOpenOk.Task.ConfigureAwait(false);
        //    await _channelHandler.Recovery(_writer).ConfigureAwait(false);
        //}
        public async Task CloseAsync(string reason = null)
        {
            //var replyText = reason == null ? "Connection closed gracefully" : reason;
            //var info = new CloseInfo(Constants.Success, replyText, 0, 0);
            //await _writer.SendConnectionCloseAsync(info).ConfigureAwait(false);
            //await _connectionCloseOkSrc.Task.ConfigureAwait(false);
            //_connectionClosedSrc.SetResult(info);
            //_cts.Cancel();
        }

        //private async Task WatchAsync()
        //{
        //    try
        //    {
        //        var info = await _connectionClosedSrc.Task.ConfigureAwait(false);                
        //        _logger.LogInformation($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
        //        _channelHandler.Stop();
        //    }
        //    catch (SocketException e)
        //    {
        //        _channelHandler.Stop(e);
        //        _logger.LogError(e.Message);
        //        _logger.LogError(e.StackTrace);
        //    }
        //    catch (IOException e)
        //    {
        //        _channelHandler.Stop(e);
        //        _logger.LogError(e.Message);
        //        _logger.LogError(e.StackTrace);
        //    }
        //    catch (RabbitMQException e)
        //    {
        //        _channelHandler.Stop(e);
        //        _logger.LogError(e.Message);
        //        _logger.LogError(e.StackTrace);
        //    }
        //    catch (Exception e)
        //    {
        //        _channelHandler.Stop(e);
        //        _logger.LogError(e.Message);
        //        _logger.LogError(e.StackTrace);
        //    }
        //    finally
        //    {
        //        _listener.Stop();
                
        //        _ctx.Abort();
        //        _heartbeat?.Dispose();
        //    }
        //    ChannelHandlerLock();
        //    try
        //    {
        //        await ReconnectAsync();
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    ChannelHandlerUnlock();


        //}
        //private void ChannelHandlerLock()
        //{
        //    foreach (var data in _channelHandler.Channels.Values)
        //    {
        //        data.waitTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        //    }
        //}
        //private void ChannelHandlerUnlock()
        //{
        //    foreach (var data in _channelHandler.Channels.Values)
        //    {
        //        data.waitTcs.SetResult(false);
        //    }
        //}
        public Task<RabbitMQChannel> OpenChannel()
        {
            return _session.OpenChannel();
        }
    }
}