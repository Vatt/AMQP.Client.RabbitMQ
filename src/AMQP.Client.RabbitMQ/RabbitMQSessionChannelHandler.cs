using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    internal sealed partial class RabbitMQSession : IChannelHandler
    {
        private readonly object lockObj = new object();
        private static int _channelId;
        private readonly TaskCompletionSource<bool> _manualCloseSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        private TaskCompletionSource<bool> _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        //private readonly SemaphoreSlim _semaphore;
        public ref TuneConf Tune => ref Options.TuneOptions;

        public void CancelTcs()
        {
            lock (lockObj)
            {
                //_semaphore.Dispose();
                if (!_openSrc.Task.IsCompleted && !_openSrc.Task.IsCanceled)
                {
                    _openSrc.SetCanceled();
                }
                if (!_manualCloseSrc.Task.IsCompleted && !_manualCloseSrc.Task.IsCanceled)
                {
                    _manualCloseSrc.SetCanceled();
                }


                foreach (var src in Channels.Values)
                {
                    //data.IsClosed = true;
                    if (src.CommonTcs != null && !src.CommonTcs.Task.IsCompleted)
                    {
                        src.CommonTcs.SetCanceled();
                    }
                    if (src.ConsumeTcs != null && !src.ConsumeTcs.Task.IsCompleted)
                    {
                        src.ConsumeTcs.SetCanceled();
                    }
                    if (src.QueueTcs != null && !src.QueueTcs.Task.IsCompleted)
                    {
                        src.QueueTcs.SetCanceled();
                    }
                    //if (src.waitTcs != null && !src.waitTcs.Task.IsCompleted)
                    //{
                    //    src.waitTcs.SetCanceled();
                    //}
                    //src.WriterSemaphore.Dispose();
                }
            }
        }
        public ValueTask OnChannelCloseAsync(ushort channelId, CloseInfo info)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnChannelCloseOkAsync(ushort channelId)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  CloseOk received");
            _manualCloseSrc.SetResult(true);
            return default;
        }

        public ValueTask OnChannelOpenOkAsync(ushort channelId)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId} OpenOk received"); ;
            _openSrc.SetResult(true);
            return default;
        }

        public ValueTask OnConsumeOkAsync(ushort channelId, string tag)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  ConsumeOk received");
            var data = GetChannelData(channelId);
            data.ConsumeTcs.SetResult(tag);
            return default;
        }

        public ValueTask OnConsumeCancelAsync(ushort channelId, ConsumeCancelInfo cancelInfo)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  ConsumeCancel received");
            var data = GetChannelData(channelId);
            if (!data.Consumers.TryGetValue(cancelInfo.ConsumerTag, out IConsumable consumer))
            {
                throw new Exception("Consumer not found");
            }
            if (cancelInfo.NoWait == false)
            {
                //TODO: отправить сигнал мб
                Debugger.Break();
            }
            return consumer.OnConsumerCancelAsync();
        }
        public ValueTask OnConsumeCancelOkAsync(ushort channelId, string tag)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  ConsumeCancelOk received");
            throw new NotImplementedException();
        }

        public ValueTask OnBeginDeliveryAsync(ushort channelId, RabbitMQDeliver deliver, ProtocolReader protocol)
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
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  ExchangeDeclareOk received");
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnExchangeDeleteOkAsync(ushort channelId)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  ExchangeDeleteOk received");
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQosOkAsync(ushort channelId)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  QosOk received");
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQueueBindOkAsync(ushort channelId)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  QueueBindOk received");
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public ValueTask OnQueueDeclareOkAsync(ushort channelId, QueueDeclareOk declare)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  QueueDeclareOk received");
            var data = GetChannelData(channelId);
            data.QueueTcs.SetResult(declare);
            return default;
        }

        public ValueTask OnQueueDeleteOkAsync(ushort channelId, int deleted)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  QueueDeleteOk received");
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(deleted);
            return default;
        }

        public ValueTask OnQueuePurgeOkAsync(ushort channelId, int purged)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  QueuePurgeOk received");
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(purged);
            return default;
        }

        public ValueTask OnQueueUnbindOkAsync(ushort channelId)
        {
            Logger.LogDebug($"{nameof(RabbitMQSession)}: ChannedId {channelId}  QueueUnbindOk received");
            var data = GetChannelData(channelId);
            data.CommonTcs.SetResult(-1);
            return default;
        }

        public async Task<RabbitMQChannel> OpenChannel()
        {
            //await _semaphore.WaitAsync().ConfigureAwait(false);
            var id = Interlocked.Increment(ref _channelId);
            _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await Writer.WriteAsync(ProtocolWriters.ChannelOpenWriter, (ushort)id).ConfigureAwait(false);
            await _openSrc.Task.ConfigureAwait(false);

            var newChannel = Channels.GetOrAdd((ushort)id, key => new RabbitMQChannel((ushort)id, this));
            //newChannel.waitTcs.SetResult(false);
            //_semaphore.Release();
            Logger.LogDebug($"{nameof(RabbitMQSession)}: Channel {id} openned");
            return newChannel;
        }

        public async Task CloseChannel(RabbitMQChannel channel, string reason = null)
        {
            //await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            var replyText = reason == null ? string.Empty : reason;
            await Writer.WriteAsync(ProtocolWriters.CloseWriter, new CloseInfo(channel.ChannelId, 20, 40, RabbitMQConstants.ReplySuccess, replyText, 0, 0)).ConfigureAwait(false);
            await _manualCloseSrc.Task.ConfigureAwait(false);
            channel.IsClosed = true;
            channel.Session = null;
            channel.WriterSemaphore = null;
            Channels.TryRemove(channel.ChannelId, out _);
            Logger.LogDebug($"{nameof(RabbitMQSession)}: Channel {channel.ChannelId} closed");
            //_writerSemaphore.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChannelData GetChannelData(ushort channelId)
        {
            if (!Channels.TryGetValue(channelId, out var data))
            {
                RabbitMQExceptionHelper.ThrowIfChannelNotFound(channelId);
            }
            return data;
        }
    }
}