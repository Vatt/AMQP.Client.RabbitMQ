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
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ
{
    public class ConnectionCloseArgs : EventArgs
    {
        public Exception? Exception { get; }
        public CloseInfo? Info { get; }
        public ConnectionCloseArgs(CloseInfo? info, Exception? exception)
        {
            Info = info;
            Exception = exception;
        }
    }
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
        
        public bool Closed { get; private set;}
        public event EventHandler<ConnectionCloseArgs> ConnectionClosed;
        
        internal RabbitMQConnection(RabbitMQConnectionFactoryBuilder builder)
        {
            Closed = false;
            _builder = builder;
            _logger = _builder.Logger;
            _channels = new ConcurrentDictionary<ushort, RabbitMQChannel>();
            _connectionClosedSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            _lockEvent = new ManualResetEventSlim(true);
            _session = new RabbitMQSession(_builder, _channels, _connectionClosedSrc, _lockEvent);
        }

        public async Task StartAsync()
        {
            ThrowIfConnectionClosed();
            await _session.Connect().ConfigureAwait(false);
            _watchTask = WatchAsync();
        }
        public async Task CloseAsync(string reason = null)
        {
            ThrowIfConnectionClosed();
            await _session.CloseAsync().ConfigureAwait(false);
            await _session.DisposeAsync().ConfigureAwait(false);
        }

        private async Task WatchAsync()
        {
            CloseInfo info = default;
            Exception exception = null;
            try
            {
                info = await _connectionClosedSrc.Task.ConfigureAwait(false);
                _logger.LogInformation($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
                _lockEvent.Reset();
            }
            catch (SocketException e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
                exception = e;
            }
            catch (IOException e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
                exception = e;
            }
            catch (RabbitMQException e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
                exception = e;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogError(e.StackTrace);
                exception = e;
            }
            finally
            {
                onConnectionClosed(new ConnectionCloseArgs(info, exception));
                if (exception != null || info.ReplyCode != RabbitMQConstants.Success )
                {
                    await ReconnectAsync().ConfigureAwait(false);
                }
                else
                {
                    Closed = true;
                }
            }
        }

        private async ValueTask ReconnectAsync()
        {
            ThrowIfConnectionClosed();
            _logger.LogDebug($"{nameof(RabbitMQConnection)}: begin reconnect");
            try
            {
                _connectionClosedSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
                _watchTask = WatchAsync();
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
            ThrowIfConnectionClosed();
            return _session.OpenChannel();
        }
        protected virtual void onConnectionClosed(ConnectionCloseArgs e)
        {
            ConnectionClosed?.Invoke(this, e);
        }

        private void ThrowIfConnectionClosed()
        {
            if (Closed)
            {
                RabbitMQExceptionHelper.ThrowConnectionClosed(ConnectionId);
            }
        }
    }
}