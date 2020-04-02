using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    class ChannelData
    {
        private Dictionary<string, QueueInfo> _queues;
        private Dictionary<string, ExchangeInfo> _exchanges;
        private Dictionary<string, QueueBindInfo> _binds;
        //private Dictionary<string, ConsumerBase> _consumers;
        public ChannelData()
        {
            _queues = new Dictionary<string, QueueInfo>();
            _exchanges = new Dictionary<string, ExchangeInfo>();
            _binds = new Dictionary<string, QueueBindInfo>();
            //_consumers = new Dictionary<string, ConsumerBase>();
        }
    }
    internal class ChannelHandler : IChannelHandler
    {
        private static int _channelId = 0;
        private readonly ConcurrentDictionary<ushort, ChannelData> _channels;
        private readonly SemaphoreSlim _writerSemaphore;
        private RabbitMQProtocolWriter _protocol;
        private TaskCompletionSource<bool> _openSrc = new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _manualCloseSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private TaskCompletionSource<CloseInfo> _channelCloseSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        public ChannelHandler(RabbitMQProtocolWriter protocol)
        {
            _protocol = protocol;
            _channels = new ConcurrentDictionary<ushort, ChannelData>();
            _writerSemaphore = new SemaphoreSlim(1);
        }
        public async Task<RabbitMQChannel> OpenChannel()
        {
            var id = Interlocked.Increment(ref _channelId);
            await _protocol.SendChannelOpenAsync((ushort)id).ConfigureAwait(false);
            await _openSrc.Task;
            _channels.GetOrAdd((ushort)id, key => new ChannelData());
            return new RabbitMQChannel((ushort)id);
        }
        public async Task CloseChannel(RabbitMQChannel channel, string? reason = null)
        {
            await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            string replyText = reason == null ? string.Empty : reason;
            await _protocol.SendClose(channel.ChannelId, 20, 40, new CloseInfo(Constants.ReplySuccess, replyText, 0, 0)).ConfigureAwait(false);
            await _manualCloseSrc.Task.ConfigureAwait(false);
            _channels.TryRemove(channel.ChannelId, out var _);
            _writerSemaphore.Release();
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
            throw new NotImplementedException();
        }

        public ValueTask OnConsumerCancelOkAsync(ushort channelId, string tag)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnDeliverAsync(ushort channelId, DeliverInfo deliver)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnExchangeDeclareOkAsync(ushort channelId)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnExchangeDeleteOkAsync(ushort channelId)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnQosOkAsync(ushort channelId)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnQueueBindOkAsync(ushort channelId)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnQueueDeclareOkAsync(ushort channelId, QueueDeclareOk declare)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnQueueDeleteOkAsync(ushort channelId, int deleted)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnQueuePurgeOkAsync(ushort channelId, int purged)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnQueueUnbindOkAsync(ushort channelId)
        {
            throw new NotImplementedException();
        }
    }
}
