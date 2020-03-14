using System.Threading;
using System.Threading.Tasks;
using System;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Channel;
using AMQP.Client.RabbitMQ.Protocol.Common;
using System.Collections.Concurrent;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System.Runtime.CompilerServices;
using AMQP.Client.RabbitMQ.Protocol.Methods.Common;
using AMQP.Client.RabbitMQ.Protocol.Internal;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection : IDisposable
    {
 
        private static int _channelId = 0; //Interlocked?
        //private readonly ConcurrentDictionary<ushort, IChannel> _channels;
        private readonly ConcurrentDictionary<ushort, RabbitMQChannel> _channels;
        private RabbitMQChannelZero Channel0;
        private TaskCompletionSource<CloseInfo> _connectionClosedSrc;
        private TaskCompletionSource<bool> _endReading;

        private Task _readingTask;      
        private Task _watchTask;      
        public RabbitMQServerInfo ServerInfo => Channel0.ServerInfo;
        public RabbitMQMainInfo MainInfo => Channel0.MainInfo;
        public RabbitMQClientInfo ClientInfo => Channel0.ClientInfo;

        private readonly RabbitMQConnectionFactoryBuilder _builder;
        private CancellationTokenSource _cts;

        private RabbitMQProtocol _protocol;
        public RabbitMQConnection(RabbitMQConnectionFactoryBuilder builder)
        {
            _builder = builder;
            _connectionClosedSrc = new TaskCompletionSource<CloseInfo>();
            _endReading = new TaskCompletionSource<bool>();
            //_channels = new ConcurrentDictionary<ushort, IChannel>();
            _channels = new ConcurrentDictionary<ushort, RabbitMQChannel>();
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            Channel0 = new RabbitMQChannelZero(_builder, _connectionClosedSrc, _cts.Token);
            await Channel0.CreateConnection().ConfigureAwait(false);
            _watchTask = Watch();
            _protocol =  new RabbitMQProtocol(Channel0.ConnectionContext);
            _readingTask = StartReading();
            await Channel0.OpenAsync(_protocol);
            
        }
        private async Task Watch()
        {
            try
            {
                var info = await _connectionClosedSrc.Task.ConfigureAwait(false);
                Console.WriteLine($"Connection closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                _endReading.SetResult(false);
                Channel0.ConnectionContext.Abort();
                _cts.Cancel();
            }
            

        }
        private async Task StartReading()
        {
            var headerReader = new FrameHeaderReader();
            try
            {
                while (true)
                {
                    var result = await _protocol.Reader.ReadAsync(headerReader, _cts.Token).ConfigureAwait(false);
                    _protocol.Reader.Advance();
                    if (result.IsCompleted)
                    {
                        break;
                    }
                    var header = result.Message;
                   
                    switch (header.FrameType)
                    {
                        case Constants.FrameMethod:
                            {
                                if (header.Channel == 0)
                                {
                                    await Channel0.HandleAsync(header).ConfigureAwait(false);
                                    break;
                                }
                                await ProcessChannels(header).ConfigureAwait(false);
                                break;
                            }
                        case Constants.FrameHeartbeat:
                            {
                                await _protocol.Reader.ReadAsync(new NoPayloadReader()).ConfigureAwait(false);
                                _protocol.Reader.Advance();
                                break;
                            }
                        default:
                            {
                                _connectionClosedSrc.SetException(new Exception($"Frame type missmatch:{header.FrameType}, {header.Channel}, {header.PaylodaSize}"));
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                _connectionClosedSrc.SetException(e);
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                //Channel0.ConnectionContext.Abort();
                //_cts.Cancel();
                //_endReading.SetResult(false);
            }

        }

        //public async ValueTask<IRabbitMQChannel> CreateChannel()
        public async ValueTask<RabbitMQChannel> CreateChannel()
        {
            var id = Interlocked.Increment(ref _channelId);
            if (id > Channel0.MainInfo.ChannelMax)
            {
                return default;
            }
            var channel = new RabbitMQChannel((ushort)id, MainInfo, _builder.PipeScheduler);
            _channels[(ushort)id] = channel;
            await channel.OpenAsync(_protocol);
            return channel;
        }
        private void RemoveChannelPrivate(ushort id)
        {
            //if (!_channels.TryRemove(id, out IChannel channel))
            if (!_channels.TryRemove(id, out RabbitMQChannel channel))
            {
                //TODO: сделать что нибудь
            }
            if (channel != null && channel.IsClosed)
            {
                //TODO: сделать что нибудь
            }
        }

        public async ValueTask CloseConnection()
        {
            await Channel0.CloseAsync("Connection closed gracefully").ConfigureAwait(false);

        }
        public Task WaitEndReading()
        {
            return _endReading.Task;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask ProcessChannels(FrameHeader header)
        {
            //if (!_channels.TryGetValue(header.Channel, out IChannel channel))
            if (!_channels.TryGetValue(header.Channel, out RabbitMQChannel channel))
            {
                throw new Exception($"{nameof(RabbitMQConnection)}: channel-id({header.Channel}) missmatch");
            }            
            return channel.HandleFrameHeaderAsync(header);
        }

        public void Dispose() => ((IDisposable)_cts).Dispose();
    }
}
