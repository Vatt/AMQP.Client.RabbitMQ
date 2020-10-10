using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Exceptions;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
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
        private TaskCompletionSource<CloseInfo> _connectionClosedSrc;
        private ManualResetEventSlim _lockEvent;
        private Task _watchTask;
        internal RabbitMQConnection(RabbitMQConnectionFactoryBuilder builder)
        {
            _builder = builder;
            _logger = _builder.Logger;
            _channels = new ConcurrentDictionary<ushort, RabbitMQChannel>();
            _connectionClosedSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            _lockEvent = new ManualResetEventSlim(true);
            _session = new RabbitMQSession(_builder, _channels, _connectionClosedSrc, _lockEvent);
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
                var info = await _connectionClosedSrc.Task.ConfigureAwait(false);
                _logger.LogInformation($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
                _lockEvent.Reset();
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
            finally
            {

            }
        }

        private async ValueTask ReconnectAsync(object? obj)
        {
            _logger.LogDebug($"{nameof(RabbitMQConnection)}: begin reconnect");
            try
            {
                _connectionClosedSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
                _watchTask = Task.Run(WatchAsync).ContinueWith(ReconnectAsync);
                await _session.DisposeAsync().ConfigureAwait(false);
                _session = new RabbitMQSession(_builder, _channels, _connectionClosedSrc, _lockEvent);
                await _session.ConnectWithRecovery().ConfigureAwait(false);


            }
            catch (Exception e)
            {
                _logger.LogDebug($"{nameof(RabbitMQConnection)}: reconnect failed with exception message {e.Message}");
                _connectionClosedSrc.SetException(e);
                Debugger.Break();
            }
            _lockEvent.Set();
            _logger.LogDebug($"{nameof(RabbitMQConnection)}: end reconnect");
        }
        public Task<RabbitMQChannel> OpenChannel()
        {
            return _session.OpenChannel();
        }
    }
}