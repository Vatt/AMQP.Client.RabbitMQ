using AMQP.Client.RabbitMQ.Protocol;
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
        private RabbitMQProtocolWriter _protocol;
        private CancellationTokenSource _cts;
        private TaskCompletionSource<CloseInfo> _connectionCloseSrc;

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
                await _listener.StartAsync(reader, this, _cts.Token);
            }
            catch (Exception e)
            {
                _connectionCloseSrc.SetException(e);
            }
        }
        private async ValueTask StartAsync(RabbitMQProtocolReader reader, RabbitMQProtocolWriter writer)
        {
            _cts = new CancellationTokenSource();
            _protocol = writer;
            _connectionCloseSrc = new TaskCompletionSource<CloseInfo>();
            StartReadingAsync(reader);
            _watchTask = WatchAsync();
            await _protocol.SendProtocol(_cts.Token);

        }
        private async Task WatchAsync()
        {
            try
            {
                var info = await _connectionCloseSrc.Task.ConfigureAwait(false);
                Console.WriteLine($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
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

        public ValueTask OnCloseOkAsync()
        {
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
        public static async Task ConnectionStartAsync(RabbitMQConnection connection)
        {
            var _client = new ClientBuilder(new ServiceCollection().BuildServiceProvider())
                                                                   .UseSockets()
                                                                   .Build();
            var ctx = await _client.ConnectAsync(connection.Options.Endpoint).ConfigureAwait(false);
            await connection.StartAsync(new RabbitMQProtocolReader(ctx), new RabbitMQProtocolWriter(ctx));
        }
        public static void ConnectionCloseAsync(RabbitMQConnection connection)
        {
            connection._connectionCloseSrc.SetResult(new CloseInfo(Constants.ReplySuccess, "Connection closed gracefully", 0, 0));
        }
    }
}
