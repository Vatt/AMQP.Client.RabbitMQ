using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Exceptions;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    internal class ChannelDataWaitSrc
    {
        public TaskCompletionSource<string> ConsumeTcs;
        public TaskCompletionSource<QueueDeclareOk> QueueTcs;
        public TaskCompletionSource<int> CommonTcs;
        public TaskCompletionSource<bool> waitTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
    internal sealed class ChannelHandler : IChannelHandler
    {
        private readonly object lockObj = new object();
        private static int _channelId;
        private readonly TaskCompletionSource<bool> _manualCloseSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private TaskCompletionSource<bool> _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly ConnectionOptions _options;

        //private TaskCompletionSource<CloseInfo> _channelCloseSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly SemaphoreSlim _semaphore;
        internal readonly ILogger Logger;
        internal readonly RabbitMQSession Session;
        internal ConcurrentDictionary<ushort, ChannelDataWaitSrc> ChannelsWaitSrc { get; }
        internal RabbitMQProtocolWriter Writer { get; private set; }
        public ref TuneConf Tune => ref _options.TuneOptions;
        public ChannelHandler(RabbitMQSession session)
        {
            Writer = session.Writer;
            _options = session.Options;
            ChannelsWaitSrc = new ConcurrentDictionary<ushort, ChannelDataWaitSrc>();
            _semaphore = new SemaphoreSlim(1);
            Logger = session.Logger;
            Session = session;
        }



        public ValueTask OnChannelCloseAsync(ushort channelId, CloseInfo info)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnChannelCloseOkAsync(ushort channelId)
        {
            Logger.LogDebug("$ChannelHandler: ChannedId {channelId}  CloseOk received");
            _manualCloseSrc.SetResult(true);
            return default;
        }

        public ValueTask OnChannelOpenOkAsync(ushort channelId)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId} OpenOk received"); ;
            _openSrc.SetResult(true);
            return default;
        }

        public ValueTask OnConsumeOkAsync(ushort channelId, string tag)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  ConsumeOk received");
            var data = GetChannelDataWaitSrc(channelId);
            data.ConsumeTcs.SetResult(tag);
            return default;
        }

        public ValueTask OnConsumeCancelAsync(ushort channelId, ConsumeCancelInfo  cancelInfo)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  ConsumeCancel received");
            var data = GetChannelData(channelId);
            if(!data.Consumers.TryGetValue(cancelInfo.ConsumerTag, out IConsumable consumer) )
            {
                throw new Exception("Consumer not found");
            }
            if(cancelInfo.NoWait == false)
            {
                //TODO: отправить сигнал мб
                Debugger.Break();
            }
            return consumer.OnConsumerCancelAsync();
        }
        public ValueTask OnConsumeCancelOkAsync(ushort channelId, string tag)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  ConsumeCancelOk received");
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
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  ExchangeDeclareOk received");
            var data = GetChannelDataWaitSrc(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnExchangeDeleteOkAsync(ushort channelId)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  ExchangeDeleteOk received");
            var data = GetChannelDataWaitSrc(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQosOkAsync(ushort channelId)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  QosOk received");
            var data = GetChannelDataWaitSrc(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQueueBindOkAsync(ushort channelId)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  QueueBindOk received");
            var data = GetChannelDataWaitSrc(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQueueDeclareOkAsync(ushort channelId, QueueDeclareOk declare)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  QueueDeclareOk received");
            var data = GetChannelDataWaitSrc(channelId);
            data.QueueTcs.SetResult(declare);
            return default;
        }

        public ValueTask OnQueueDeleteOkAsync(ushort channelId, int deleted)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  QueueDeleteOk received");
            var data = GetChannelDataWaitSrc(channelId);
            data.CommonTcs.SetResult(deleted);
            return default;
        }

        public ValueTask OnQueuePurgeOkAsync(ushort channelId, int purged)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  QueuePurgeOk received");
            var data = GetChannelDataWaitSrc(channelId);
            data.CommonTcs.SetResult(purged);
            return default;
        }

        public ValueTask OnQueueUnbindOkAsync(ushort channelId)
        {
            Logger.LogDebug($"ChannelHandler: ChannedId {channelId}  QueueUnbindOk received");
            var data = GetChannelDataWaitSrc(channelId);
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
            Session.Channels.GetOrAdd((ushort)id, key => new ChannelData());
            ChannelsWaitSrc.GetOrAdd((ushort)id, key => new ChannelDataWaitSrc());
            var newChannel = GetChannelDataWaitSrc((ushort)id); //TODO: fix this shit
            newChannel.waitTcs.SetResult(false);
            _semaphore.Release();
            Logger.LogDebug($"ChannelHandler: Channel {id} openned");
            return new RabbitMQChannel((ushort)id, this);
        }

        public async Task CloseChannel(RabbitMQChannel channel, string reason = null)
        {
            //await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            var replyText = reason == null ? string.Empty : reason;
            await Writer.SendClose(channel.ChannelId, 20, 40, new CloseInfo(Constants.ReplySuccess, replyText, 0, 0))
                .ConfigureAwait(false);
            await _manualCloseSrc.Task.ConfigureAwait(false);
            Session.Channels.TryRemove(channel.ChannelId, out _);
            Logger.LogDebug($"ChannelHandler: Channel {channel.ChannelId} closed");
            //_writerSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChannelData GetChannelData(ushort channelId)
        {
            if (!Session.Channels.TryGetValue(channelId, out var data))
            {
                RabbitMQExceptionHelper.ThrowIfChannelNotFound();
            }
            return data;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChannelDataWaitSrc GetChannelDataWaitSrc(ushort channelId)
        {
            if (!ChannelsWaitSrc.TryGetValue(channelId, out var data))
            {
                RabbitMQExceptionHelper.ThrowIfChannelNotFound();
            }
            return data;
        }
        //public void Stop(Exception reason)
        //{
        //    lock(lockObj)
        //    {
        //        Logger.LogDebug($"{nameof(ChannelHandler)}: Stop with {reason.Message}");
        //        foreach (var data in ChannelsWaitSrc.Values)
        //        {
        //            data.IsClosed = true;
        //            if (data.CommonTcs != null && !data.CommonTcs.Task.IsCompleted)
        //            {
        //                data.CommonTcs.SetException(reason);
        //            }
        //            if (data.ConsumeTcs != null && !data.ConsumeTcs.Task.IsCompleted)
        //            {
        //                data.ConsumeTcs.SetException(reason);
        //            }
        //            if (data.QueueTcs != null && !data.QueueTcs.Task.IsCompleted)
        //            {
        //                data.QueueTcs.SetException(reason);
        //            }
        //            if (data.waitTcs != null && !data.waitTcs.Task.IsCompleted)
        //            {
        //                data.waitTcs.SetException(reason);
        //            }
        //            data.WriterSemaphore.Dispose();
        //        }
        //    }
        //}
        //public void Stop()
        //{            
        //    lock (lockObj)
        //    {
        //        Logger.LogDebug($"{nameof(ChannelHandler)}: Stop ");
        //        foreach (var data in Channels.Values)
        //        {
        //            data.IsClosed = true;
        //            if (data.CommonTcs != null && !data.CommonTcs.Task.IsCompleted)
        //            {
        //                data.CommonTcs.SetCanceled();
        //            }
        //            if (data.ConsumeTcs != null && !data.ConsumeTcs.Task.IsCompleted)
        //            {
        //                data.ConsumeTcs.SetCanceled();
        //            }
        //            if (data.QueueTcs != null && !data.QueueTcs.Task.IsCompleted)
        //            {
        //                data.QueueTcs.SetCanceled();
        //            }
        //            if (data.waitTcs!= null && !data.waitTcs.Task.IsCompleted)
        //            {
        //                data.waitTcs.SetCanceled();
        //            }
        //            data.WriterSemaphore.Dispose();
        //        }
        //    }
        //}
        //public async ValueTask Recovery(RabbitMQProtocolWriter writer)
        //{
        //    Writer = writer;
        //    foreach (var channelPair in Channels)
        //    {
        //        var channel = channelPair.Value;
        //        await channel.WriterSemaphore.WaitAsync();

        //        _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        //        await writer.SendChannelOpenAsync(channelPair.Key).ConfigureAwait(false);
        //        await _openSrc.Task.ConfigureAwait(false);

        //        foreach (var exchange in channelPair.Value.Exchanges.Values)
        //        {
        //            if (exchange.NoWait)
        //            {
        //                await writer.SendExchangeDeclareAsync(channelPair.Key, exchange).ConfigureAwait(false);
        //                continue;
        //            }
        //            else
        //            {
        //                channel.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        //                await writer.SendExchangeDeclareAsync(channelPair.Key, exchange).ConfigureAwait(false);
        //                await channelPair.Value.CommonTcs.Task.ConfigureAwait(false);
        //            }

        //        }
        //        foreach (var queue in channelPair.Value.Queues.Values)
        //        {
        //            if (queue.NoWait)
        //            {
        //                await writer.SendQueueDeclareAsync(channelPair.Key, queue).ConfigureAwait(false);
        //                continue;
        //            }
        //            else
        //            {
        //                channel.QueueTcs = new TaskCompletionSource<QueueDeclareOk>(TaskCreationOptions.RunContinuationsAsynchronously);
        //                await writer.SendQueueDeclareAsync(channelPair.Key, queue).ConfigureAwait(false);
        //                var declare = await channel.QueueTcs.Task.ConfigureAwait(false);
        //            }

        //        }
        //        foreach (var bind in channelPair.Value.Binds.Values)
        //        {
        //            if (bind.NoWait)
        //            {
        //                await writer.SendQueueBindAsync(channelPair.Key, bind).ConfigureAwait(false);
        //                continue;
        //            }
        //            else
        //            {
        //                channel.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        //                await writer.SendQueueBindAsync(channelPair.Key, bind).ConfigureAwait(false);
        //                await channelPair.Value.CommonTcs.Task.ConfigureAwait(false);
        //            }
        //        }
        //        foreach (var consumer in channelPair.Value.Consumers.Values)
        //        {
        //            if (consumer.Conf.NoWait)
        //            {
        //                await writer.SendBasicConsumeAsync(channelPair.Key, consumer.Conf).ConfigureAwait(false);
        //                continue;
        //            }
        //            else
        //            {
        //                channel.ConsumeTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        //                await writer.SendBasicConsumeAsync(channelPair.Key, consumer.Conf).ConfigureAwait(false);
        //                var tag = await channel.ConsumeTcs.Task.ConfigureAwait(false);
        //                if (!tag.Equals(consumer.Conf.ConsumerTag))
        //                {
        //                    RabbitMQExceptionHelper.ThrowIfConsumeOkTagMissmatch(consumer.Conf.ConsumerTag, tag);
        //                }
        //            }
        //        }
        //        channel.IsClosed = false;
        //        channel.WriterSemaphore.Release();
        //    }
        //}

    }
}