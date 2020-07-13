using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    internal class ChannelData
    {
        public Dictionary<string, QueueBind> Binds = new Dictionary<string, QueueBind>();        
        public Dictionary<string, IConsumable> Consumers = new Dictionary<string, IConsumable>();
        public Dictionary<string, ExchangeDeclare> Exchanges = new Dictionary<string, ExchangeDeclare>();
        public Dictionary<string, QueueDeclare> Queues = new Dictionary<string, QueueDeclare>();
        public TaskCompletionSource<string> ConsumeTcs;
        public TaskCompletionSource<QueueDeclareOk> QueueTcs;
        public TaskCompletionSource<int> CommonTcs;
        public SemaphoreSlim WriterSemaphore = new SemaphoreSlim(1);

    }

    internal class ChannelHandler : IChannelHandler
    {
        private static int _channelId;
        private readonly TaskCompletionSource<bool> _manualCloseSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private TaskCompletionSource<bool> _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly ConnectionOptions _options;

        //private TaskCompletionSource<CloseInfo> _channelCloseSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly SemaphoreSlim _semaphore;
        internal ConcurrentDictionary<ushort, ChannelData> Channels { get; }
        internal RabbitMQProtocolWriter Writer { get; private set; }
        public ref TuneConf Tune => ref _options.TuneOptions;
        public ChannelHandler(RabbitMQProtocolWriter writer, ConnectionOptions options)
        { 
            Writer = writer;
            _options = options;
            Channels = new ConcurrentDictionary<ushort, ChannelData>();
            _semaphore = new SemaphoreSlim(1);
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


        public ValueTask OnBeginDeliveryAsync(ushort channelId, RabbitMQDeliver deliver, RabbitMQProtocolReader protocol)
        {
            var data = GetChannelData(channelId);
            if (!data.Consumers.TryGetValue(deliver.ConsumerTag, out var consumer))
            {
                throw new Exception("Consumer not found");
            }
            return consumer.OnBeginDeliveryAsync(deliver, protocol);
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
            _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await Writer.SendChannelOpenAsync((ushort)id).ConfigureAwait(false);
            await _openSrc.Task.ConfigureAwait(false);
            Channels.GetOrAdd((ushort)id, key => new ChannelData());
            _semaphore.Release();
            return new RabbitMQChannel((ushort)id, this);
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
            if (!Channels.TryGetValue(channelId, out var data))
            {
                RabbitMQExceptionHelper.ThrowIfChannelNotFound();
            }
            return data;
        }

        public async ValueTask Recovery(RabbitMQProtocolWriter writer)
        {
            Writer = writer;
            foreach(var channelPair in Channels)
            {
                var channel = channelPair.Value;
                await channel.WriterSemaphore.WaitAsync();

                _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                await writer.SendChannelOpenAsync(channelPair.Key).ConfigureAwait(false);
                await _openSrc.Task.ConfigureAwait(false);

                foreach (var exchange in channelPair.Value.Exchanges.Values)
                {
                    if (exchange.NoWait)
                    {
                        await writer.SendExchangeDeclareAsync(channelPair.Key, exchange).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        channel.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                        await writer.SendExchangeDeclareAsync(channelPair.Key, exchange).ConfigureAwait(false);
                        await channelPair.Value.CommonTcs.Task.ConfigureAwait(false);
                    }

                }
                foreach (var queue in channelPair.Value.Queues.Values)
                {
                    if (queue.NoWait)
                    {
                        await writer.SendQueueDeclareAsync(channelPair.Key, queue).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        channel.QueueTcs = new TaskCompletionSource<QueueDeclareOk>(TaskCreationOptions.RunContinuationsAsynchronously);
                        await writer.SendQueueDeclareAsync(channelPair.Key, queue).ConfigureAwait(false);
                        var declare = await channel.QueueTcs.Task.ConfigureAwait(false);
                    }
                    
                }
                foreach(var bind in channelPair.Value.Binds.Values)
                {
                    if (bind.NoWait)
                    {
                        await writer.SendQueueBindAsync(channelPair.Key, bind).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        channel.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
                        await writer.SendQueueBindAsync(channelPair.Key, bind).ConfigureAwait(false);
                        await channelPair.Value.CommonTcs.Task.ConfigureAwait(false);
                    }
                }
                foreach(var consumer in channelPair.Value.Consumers.Values)
                {
                    if (consumer.Conf.NoWait)
                    {
                        await writer.SendBasicConsumeAsync(channelPair.Key, consumer.Conf).ConfigureAwait(false);
                        continue;
                    }
                    else
                    {
                        channel.ConsumeTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
                        await writer.SendBasicConsumeAsync(channelPair.Key, consumer.Conf).ConfigureAwait(false);
                        var tag = await channel.ConsumeTcs.Task.ConfigureAwait(false);
                        if (!tag.Equals(consumer.Conf.ConsumerTag))
                        {
                            RabbitMQExceptionHelper.ThrowIfConsumeOkTagMissmatch(consumer.Conf.ConsumerTag, tag);
                        }
                    }
                }
                channel.WriterSemaphore.Release();
            }
        }
    }
}