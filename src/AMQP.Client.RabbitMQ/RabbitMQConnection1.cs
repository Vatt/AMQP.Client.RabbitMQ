namespace AMQP.Client.RabbitMQ
{
    /*
    public class RabbitMQConnection1 : IDisposable
    {

        private static int _channelId = 0;
        private readonly ConcurrentDictionary<ushort, RabbitMQChannel> _channels;
        private RabbitMQChannelZero Channel0;
        private TaskCompletionSource<CloseInfo> _connectionClosedSrc;

        private Task _readingTask;
        private Task _watchTask;
        public ServerConf ServerInfo => Channel0.ServerInfo;
        public TuneConf MainInfo => Channel0.MainInfo;
        public ClientConf ClientInfo => Channel0.ClientInfo;

        private readonly RabbitMQConnectionFactoryBuilder1 _builder;
        private CancellationTokenSource _cts;

        private RabbitMQProtocolWriter _protocol;
        public RabbitMQConnection1(RabbitMQConnectionFactoryBuilder1 builder)
        {
            _builder = builder;
            _connectionClosedSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
            _channels = new ConcurrentDictionary<ushort, RabbitMQChannel>();
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            Channel0 = new RabbitMQChannelZero(_builder, _connectionClosedSrc, _cts.Token);
            await Channel0.CreateConnection().ConfigureAwait(false);
            _watchTask = Watch();
            _protocol = new RabbitMQProtocolWriter(Channel0.ConnectionContext);
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                Channel0.ConnectionContext.Abort();
                _cts.Cancel();
            }


        }
        private async Task StartReading()
        {
            try
            {
                while (true)
                {
                    var frameHeader = await _protocol.ReadFrameHeader(_cts.Token);

                    switch (frameHeader.FrameType)
                    {
                        case Constants.FrameMethod:
                            {
                                if (frameHeader.Channel == 0)
                                {
                                    await Channel0.HandleAsync(frameHeader).ConfigureAwait(false);
                                    break;
                                }
                                await ProcessChannels(frameHeader).ConfigureAwait(false);
                                break;
                            }
                        case Constants.FrameHeartbeat:
                            {
                                await _protocol.ReadNoPayload().ConfigureAwait(false);
                                break;
                            }
                        default:
                            {
                                _connectionClosedSrc.SetException(new Exception($"Frame type missmatch:{frameHeader.FrameType}, {frameHeader.Channel}, {frameHeader.PaylodaSize}"));
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

            var channel = _channels.GetOrAdd((ushort)id, key => new RabbitMQChannel((ushort)id, MainInfo, _builder.PipeScheduler));
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ValueTask ProcessChannels(FrameHeader header)
        {
            //if (!_channels.TryGetValue(header.Channel, out IChannel channel))
            if (!_channels.TryGetValue(header.Channel, out RabbitMQChannel channel))
            {
                throw new Exception($"{nameof(RabbitMQConnection1)}: channel-id({header.Channel}) missmatch");
            }
            return channel.HandleFrameHeaderAsync(header);
        }

        public void Dispose() => ((IDisposable)_cts).Dispose();
    }
    */
}
