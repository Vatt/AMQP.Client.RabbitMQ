using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    class ChannelData
    {
        public TaskCompletionSource<int> CommonTcs;
        public TaskCompletionSource<QueueDeclareOk> QueueTcs;
        public TaskCompletionSource<string> ConsumeTcs;
        public IConsumable ActiveConsumer;
        public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);
        public Dictionary<string, QueueDeclare> Queues = new Dictionary<string, QueueDeclare>();
        public Dictionary<string, ExchangeDeclare> Exchanges = new Dictionary<string, ExchangeDeclare>();
        public Dictionary<string, QueueBind> Binds = new Dictionary<string, QueueBind>();
        public Dictionary<string, IConsumable> Consumers = new Dictionary<string, IConsumable>();
        public ChannelData()
        {
        }
    }
    internal class ChannelHandler : IChannelHandler
    {

        private static int _channelId = 0;
        private readonly ConcurrentDictionary<ushort, ChannelData> _channels;
        private RabbitMQProtocolWriter _writer;
        internal ConcurrentDictionary<ushort, ChannelData> Channels => _channels;
        internal RabbitMQProtocolWriter Writer => _writer;

        private TaskCompletionSource<bool> _openSrc = new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _manualCloseSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        //private TaskCompletionSource<CloseInfo> _channelCloseSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        public ChannelHandler(RabbitMQProtocolWriter writer)
        {
            _writer = writer;
            _channels = new ConcurrentDictionary<ushort, ChannelData>();
            //_writerSemaphore = new SemaphoreSlim(1);
        }
        public async Task<RabbitMQChannel> OpenChannel()
        {
            var id = Interlocked.Increment(ref _channelId);
            await _writer.SendChannelOpenAsync((ushort)id).ConfigureAwait(false);
            await _openSrc.Task.ConfigureAwait(false);
            _channels.GetOrAdd((ushort)id, key => new ChannelData());
            return new RabbitMQChannel((ushort)id, this);
        }
        public async Task CloseChannel(RabbitMQChannel channel, string reason = null)
        {
            //await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            string replyText = reason == null ? string.Empty : reason;
            await _writer.SendClose(channel.ChannelId, 20, 40, new CloseInfo(Constants.ReplySuccess, replyText, 0, 0)).ConfigureAwait(false);
            await _manualCloseSrc.Task.ConfigureAwait(false);
            _channels.TryRemove(channel.ChannelId, out var _);
            //_writerSemaphore.Release();
        }
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
            {
                throw new Exception("Consumer not found");
            }
            data.ActiveConsumer = consumer;
            return consumer.OnDeliveryAsync(ref deliver);
        }
        public ValueTask OnContentHeaderAsync(ushort channelId, ContentHeader header)
        {
            var data = GetChannelData(channelId);
            data.ActiveConsumer.OnContentAsync(ref header);
            return default;
        }
        public ValueTask OnBodyAsync(ushort channelId, ReadOnlySequence<byte> chunk)
        {
            var data = GetChannelData(channelId);
            data.ActiveConsumer.OnBodyAsync(chunk);
            return default;
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
            throw new NotImplementedException();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChannelData GetChannelData(ushort channelId)
        {
            if (!_channels.TryGetValue(channelId, out var data))
            {
                RabbitMQExceptionHelper.ThrowIfChannelNotFound();
            }
            return data;
        }
    }
}
