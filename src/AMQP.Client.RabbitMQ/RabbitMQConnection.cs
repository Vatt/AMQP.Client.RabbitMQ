using AMQP.Client.RabbitMQ.Protocol.Exceptions;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection
    {

        private RabbitMQSession _session;
        //public EventHandler ConnectionBlocked;
        //public EventHandler ConnectionUnblocked;
        //public EventHandler ConnenctionClosed;
        private ConcurrentDictionary<ushort, RabbitMQChannel> _channels;
        public ref readonly ConnectionOptions Options => ref _session.Options;
        public ref readonly ServerConf ServerOptions => ref _session.ServerOptions;
        public ref readonly Guid ConnectionId => ref _session.ConnectionId;
        private ILogger _logger;
        private RabbitMQConnectionFactoryBuilder _builder;

        private Task _watchTask;
        internal RabbitMQConnection(RabbitMQConnectionFactoryBuilder builder)
        {
            _builder = builder;
            _logger = _builder.Logger;
            _channels = new ConcurrentDictionary<ushort, RabbitMQChannel>();
            _session = new RabbitMQSession(_builder, _channels);
        }

        public async Task StartAsync()
        {
            await _session.Connect().ConfigureAwait(false);
            _watchTask = Task.Run(WatchAsync).ContinueWith(ReconnectAsync);
        }
        public async Task CloseAsync(string reason = null)
        {
            await _session.CloseAsync().ConfigureAwait(false);
            await _session.DisposeAsync().ConfigureAwait(false);
        }

        private async Task WatchAsync()
        {
            try
            {
                var info = await _session.ConnectionClosedSrc.Task.ConfigureAwait(false);
                _logger.LogInformation($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
            }
            catch (SocketException e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
            }
            catch (IOException e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
            }
            catch (RabbitMQException e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
            }
        }

        private async ValueTask ReconnectAsync(object? obj)
        {
            ChannelHandlerLock();
            try
            {
                await _session.DisposeAsync().ConfigureAwait(false);
                _session = new RabbitMQSession(_builder, _channels);
                await _session.ConnectWithRecovery().ConfigureAwait(false);
                _watchTask = Task.Run(WatchAsync).ContinueWith(ReconnectAsync);
            }
            catch (Exception e)
            {
                Debugger.Break();
            }
            ChannelHandlerUnlock();
        }

        private void ChannelHandlerLock()
        {
            foreach (var data in _session.Channels.Values)
            {
                data.waitTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }
        private void ChannelHandlerUnlock()
        {
            foreach (var data in _session.Channels.Values)
            {
                data.waitTcs.SetResult(false);
            }
        }
        public Task<RabbitMQChannel> OpenChannel()
        {
            return _session.OpenChannel();
        }
    }
}