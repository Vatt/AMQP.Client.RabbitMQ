﻿using AMQP.Client.RabbitMQ.Channel;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public abstract class ConsumerBase
    {
        public ushort ChannelId => Channel.ChannelId;
        public RabbitMQChannel Channel { get; }
        protected readonly RabbitMQProtocol _protocol;
        internal TaskCompletionSource<string> CancelSrc;
        internal TaskCompletionSource<string> ConsumeOkSrc;
        private ConsumerInfo _info;
        private SemaphoreSlim _semaphore;
        private ContentHeaderFullReader _contentHeaderFullReader;

        public bool IsCanceled { get; protected set; }
        internal ConsumerBase(ConsumerInfo info, RabbitMQProtocol protocol, RabbitMQChannel channel)
        {
            Channel = channel;
            _protocol = protocol;
            _info = info;
            ConsumeOkSrc = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            IsCanceled = true;
            _semaphore = new SemaphoreSlim(1);
            _contentHeaderFullReader = new ContentHeaderFullReader(channel.ChannelId);
        }
        public async ValueTask ConsumerStartAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            await _protocol.Writer.WriteAsync(new BasicConsumeWriter(ChannelId), _info).ConfigureAwait(false);
            await ConsumeOkSrc.Task.ConfigureAwait(false);
            IsCanceled = false;
            _semaphore.Release();
        }
        public async ValueTask<string> CancelAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);            
            CancelSrc = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);            
            await _protocol.Writer.WriteAsync(new BasicConsumeCancelWriter(ChannelId), new ConsumeCancelInfo(_info.ConsumerTag, false)).ConfigureAwait(false);
            var result = await CancelSrc.Task.ConfigureAwait(false);
            IsCanceled = true;
            _semaphore.Release();
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask Delivery(DeliverInfo info)
        {
            if (IsCanceled)
            {
                throw new Exception("Consumer already canceled");
            }
            var contentResult = await _protocol.Reader.ReadAsync(_contentHeaderFullReader).ConfigureAwait(false);
            _protocol.Reader.Advance();
            await ProcessBodyMessage(new RabbitMQDeliver(info.DeliverTag, contentResult.Message)).ConfigureAwait(false);
        }
        internal abstract ValueTask ProcessBodyMessage(RabbitMQDeliver deliver);

    }
}
