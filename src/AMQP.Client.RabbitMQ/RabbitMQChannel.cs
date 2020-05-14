﻿using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQChannel : IDisposable
    {
        private static readonly int _publishBatchSize = 4;
        private readonly ChannelHandler _handler;
        private readonly ReadOnlyMemory<byte>[] _publishBatch;
        private readonly SemaphoreSlim _writerSemaphore;
        public readonly ushort ChannelId;

        internal RabbitMQChannel(ushort id, ChannelHandler handler)
        {
            ChannelId = id;
            _handler = handler;
            _publishBatch = new ReadOnlyMemory<byte>[_publishBatchSize];
            _writerSemaphore = new SemaphoreSlim(1);
        }

        public void Dispose()
        {
            _writerSemaphore.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ExchangeDeclareAsync(ExchangeDeclare exchange)
        {
            return _handler.ExchangeDeclareAsync(this, exchange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ExchangeDeleteAsync(ExchangeDelete exchange)
        {
            return _handler.ExchangeDeleteAsync(this, exchange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<QueueDeclareOk> QueueDeclareAsync(QueueDeclare queue)
        {
            return _handler.QueueDeclareAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueDeclareNoWaitAsync(QueueDeclare queue)
        {
            queue.NoWait = true;
            return _handler.QueueDeclareNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> QueueDeleteAsync(QueueDelete queue)
        {
            return _handler.QueueDeleteAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueDeleteNoWaitAsync(QueueDelete queue)
        {
            queue.NoWait = true;
            return _handler.QueueDeleteNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> QueuePurgeAsync(QueuePurge queue)
        {
            return _handler.QueuePurgeAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueuePurgeNoWaitAsync(QueuePurge queue)
        {
            queue.NoWait = true;
            return _handler.QueuePurgeNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueBindAsync(QueueBind bind)
        {
            return _handler.QueueBindAsync(this, bind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueUnbindAsync(QueueUnbind unbind)
        {
            return _handler.QueueUnbindAsync(this, unbind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ConsumerStartAsync(RabbitMQConsumer consumer)
        {
            return _handler.ConsumerStartAsync(consumer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask Publish(string exchangeName, string routingKey, bool mandatory, bool immediate,
            ContentHeaderProperties properties, ReadOnlyMemory<byte> message)
        {
            //if (IsClosed)
            //{
            //    throw new Exception($"{nameof(RabbitMQChannel)}.{nameof(Publish)}: channel is canceled");
            //}
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(60, message.Length, ref properties);

            if (message.Length <= _handler.Tune.FrameMax)
            {
                var allinfo = new PublishAllInfo(message, ref info, ref content);
                await _handler.Writer.PublishAllAsync(ChannelId, allinfo).ConfigureAwait(false);
                return;
            }


            await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            var written = 0;
            var partialInfo = new PublishPartialInfo(ref info, ref content);
            await _handler.Writer.PublishPartialAsync(ChannelId, partialInfo).ConfigureAwait(false);
            while (written < content.BodySize)
            {
                var batchCnt = 0;
                while (batchCnt < _publishBatchSize && written < content.BodySize)
                {
                    var writable = Math.Min(_handler.Tune.FrameMax, (int)content.BodySize - written);
                    _publishBatch[batchCnt] = message.Slice(written, writable);
                    batchCnt++;
                    written += writable;
                }

                await _handler.Writer.PublishBodyAsync(ChannelId, _publishBatch).ConfigureAwait(false);
                _publishBatch.AsSpan().Fill(ReadOnlyMemory<byte>.Empty);
            }

            _writerSemaphore.Release();
        }

        public ValueTask Ack(AckInfo ack)
        {
            return _handler.Writer.SendAckAsync(ChannelId, ref ack);
        }

        public ValueTask Reject(RejectInfo reject)
        {
            //if (IsClosed)
            //{
            //    throw new Exception($"{nameof(RabbitMQChannel)}.{nameof(Reject)}: channel is canceled");
            //}
            return _handler.Writer.SendRejectAsync(ChannelId, ref reject);
        }

        public Task QoS(QoSInfo qos)
        {
            return _handler.QoS(this, qos);
        }
    }
}