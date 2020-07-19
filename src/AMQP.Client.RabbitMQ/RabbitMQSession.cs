using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    class RabbitMQSession : IConnectionHandler,  IAsyncDisposable
    {
        private RabbitMQListener _listener;
        private CancellationTokenSource _cts;
        private ConnectionContext _ctx;
        private Timer _heartbeat;
        private Task _readingTask;
        private Task _watchTask;
        private TaskCompletionSource<bool> _connectionCloseOkSrc;
        private TaskCompletionSource<bool> _connectionOpenOk;
        private TaskCompletionSource<CloseInfo> _connectionClosedSrc;
        private ILogger _logger;
        public readonly ConnectionOptions Options;
        public ServerConf ServerOptions;
        public readonly Guid ConnectionId;
        public RabbitMQProtocolWriter Writer { get; }
        public RabbitMQProtocolReader Reader { get; }
        public RabbitMQSession(RabbitMQConnectionFactoryBuilder builder)
        {
            Options = builder.Options;
            _logger = builder.Logger;
            ConnectionId = Guid.NewGuid();
        }

        public async ValueTask DisposeAsync()
        {
            _heartbeat?.Dispose();            
            await Writer.DisposeAsync();
            await Reader.DisposeAsync();
            await _ctx.DisposeAsync();

        }

        ValueTask IConnectionHandler.OnCloseAsync(CloseInfo info)
        {
            _logger.LogDebug($"RabbitMQConnection {ConnectionId}: Close received");
            _connectionClosedSrc.SetResult(info);
            return default;
        }

        ValueTask IConnectionHandler.OnCloseOkAsync()
        {
            _logger.LogDebug($"RabbitMQConnection {ConnectionId}: CloseOk received");
            _connectionCloseOkSrc.SetResult(true);
            return default;
        }

        ValueTask IConnectionHandler.OnHeartbeatAsync()
        {
            return default;
        }

        ValueTask IConnectionHandler.OnOpenOkAsync()
        {
            _logger.LogDebug($"RabbitMQConnection {ConnectionId}: OpenOk received");
            _heartbeat = new Timer(Heartbeat, null, 0, Options.TuneOptions.Heartbeat);
            _connectionOpenOk.SetResult(true);
            return default;
        }

        async ValueTask IConnectionHandler.OnStartAsync(ServerConf conf)
        {
            _logger.LogDebug($"RabbitMQConnection {ConnectionId}: Start received");
            ServerOptions = conf;
            await Writer.SendStartOkAsync(Options.ClientOptions, Options.ConnOptions).ConfigureAwait(false);
        }

        async ValueTask IConnectionHandler.OnTuneAsync(TuneConf conf)
        {
            _logger.LogDebug($"RabbitMQConnection {ConnectionId}: Tune received");
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
    }
}
