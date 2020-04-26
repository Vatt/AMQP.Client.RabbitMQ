using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;

namespace AMQP.Client.RabbitMQ
{
    internal class ChannelData
    {
        public IConsumable ActiveConsumer;
        public Dictionary<string, QueueBind> Binds = new Dictionary<string, QueueBind>();
        public TaskCompletionSource<int> CommonTcs;
        public Dictionary<string, IConsumable> Consumers = new Dictionary<string, IConsumable>();
        public TaskCompletionSource<string> ConsumeTcs;
        public Dictionary<string, ExchangeDeclare> Exchanges = new Dictionary<string, ExchangeDeclare>();
        public Dictionary<string, QueueDeclare> Queues = new Dictionary<string, QueueDeclare>();
        public TaskCompletionSource<QueueDeclareOk> QueueTcs;
    }

    internal class ChannelHandler : IChannelHandler
    {
        private static int _channelId;
        private ChannelData _activeChannelForConsume;

        private readonly TaskCompletionSource<bool> _manualCloseSrc =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private TaskCompletionSource<bool> _openSrc = new TaskCompletionSource<bool>();

        private readonly ConnectionOptions _options;

        //private TaskCompletionSource<CloseInfo> _channelCloseSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly SemaphoreSlim _semaphore;

        public ChannelHandler(RabbitMQProtocolWriter writer, ConnectionOptions options)
        {
            Writer = writer;
            _options = options;
            Channels = new ConcurrentDictionary<ushort, ChannelData>();
            _semaphore = new SemaphoreSlim(1);
        }

        internal ConcurrentDictionary<ushort, ChannelData> Channels { get; }

        internal RabbitMQProtocolWriter Writer { get; }

        public ref TuneConf Tune => ref _options.TuneOptions;

        public ValueTask OnChannelCloseAsync(ushort channelId, CloseInfo info)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnChannelCloseOkAsync(ushort channelId)
        {
            _manualCloseSrc.SetResult(true);
            return default;
        }

        public ValueTask OnChannelOpenOkAsync(ushort channelId)
        {
            _openSrc.SetResult(true);
            return default;
        }

        public ValueTask OnConsumeOkAsync(ushort channelId, string tag)
        {
            var data = GetChannelData(channelId);
            data.ConsumeTcs.SetResult(tag);
            return default;
        }

        public ValueTask OnConsumerCancelOkAsync(ushort channelId, string tag)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnDeliverAsync(ushort channelId, Deliver deliver)
        {
            var data = GetChannelData(channelId);
            if (!data.Consumers.TryGetValue(deliver.ConsumerTag, out var consumer))
                throw new Exception("Consumer not found");
            _activeChannelForConsume = data;
            data.ActiveConsumer = consumer;
            return consumer.OnDeliveryAsync(ref deliver);
        }

        public ValueTask OnContentHeaderAsync(ushort channelId, ContentHeader header)
        {
            //var data = GetChannelData(channelId);

            return _activeChannelForConsume.ActiveConsumer.OnContentAsync(header);
        }

        public ValueTask OnBodyAsync(ushort channelId, ReadOnlySequence<byte> chunk)
        {
            //var data = GetChannelData(channelId);
            return _activeChannelForConsume.ActiveConsumer.OnBodyAsync(chunk);
        }

        public ValueTask OnExchangeDeclareOkAsync(ushort channelId)
        {
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnExchangeDeleteOkAsync(ushort channelId)
        {
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQosOkAsync(ushort channelId)
        {
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQueueBindOkAsync(ushort channelId)
        {
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQueueDeclareOkAsync(ushort channelId, QueueDeclareOk declare)
        {
            var data = GetChannelData(channelId);
            data.QueueTcs.SetResult(declare);
            return default;
        }

        public ValueTask OnQueueDeleteOkAsync(ushort channelId, int deleted)
        {
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(deleted);
            return default;
        }

        public ValueTask OnQueuePurgeOkAsync(ushort channelId, int purged)
        {
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(purged);
            return default;
        }

        public ValueTask OnQueueUnbindOkAsync(ushort channelId)
        {
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public async Task<RabbitMQChannel> OpenChannel()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            var id = Interlocked.Increment(ref _channelId);
            _openSrc = new TaskCompletionSource<bool>();
            await Writer.SendChannelOpenAsync((ushort) id).ConfigureAwait(false);
            await _openSrc.Task.ConfigureAwait(false);
            Channels.GetOrAdd((ushort) id, key => new ChannelData());
            _semaphore.Release();
            return new RabbitMQChannel((ushort) id, this);
        }

        public async Task CloseChannel(RabbitMQChannel channel, string reason = null)
        {
            //await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            var replyText = reason == null ? string.Empty : reason;
            await Writer.SendClose(channel.ChannelId, 20, 40, new CloseInfo(Constants.ReplySuccess, replyText, 0, 0))
                .ConfigureAwait(false);
            await _manualCloseSrc.Task.ConfigureAwait(false);
            Channels.TryRemove(channel.ChannelId, out _);
            //_writerSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChannelData GetChannelData(ushort channelId)
        {
            if (!Channels.TryGetValue(channelId, out var data)) RabbitMQExceptionHelper.ThrowIfChannelNotFound();
            return data;
        }
    }
}