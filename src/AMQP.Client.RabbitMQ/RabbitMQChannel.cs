using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;

namespace AMQP.Client.RabbitMQ
{
    public sealed class RabbitMQChannel : ChannelData
    {
        private readonly ReadOnlyMemory<byte>[] _publishBatch;
        private static readonly int _publishBatchSize = 4;
        public readonly ushort ChannelId;

        internal RabbitMQChannel(ushort id, RabbitMQSession session) : base(session)
        {
            ChannelId = id;
            _publishBatch = new ReadOnlyMemory<byte>[_publishBatchSize];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ExchangeDeclareAsync(ExchangeDeclare exchange)
        {
            return Session.ExchangeDeclareAsync(this, exchange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ExchangeDeleteAsync(ExchangeDelete exchange)
        {
            return Session.ExchangeDeleteAsync(this, exchange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<QueueDeclareOk> QueueDeclareAsync(QueueDeclare queue)
        {
            return Session.QueueDeclareAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueDeclareNoWaitAsync(QueueDeclare queue)
        {
            queue.NoWait = true;
            return Session.QueueDeclareNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> QueueDeleteAsync(QueueDelete queue)
        {
            return Session.QueueDeleteAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueDeleteNoWaitAsync(QueueDelete queue)
        {
            queue.NoWait = true;
            return Session.QueueDeleteNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> QueuePurgeAsync(QueuePurge queue)
        {
            return Session.QueuePurgeAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueuePurgeNoWaitAsync(QueuePurge queue)
        {
            queue.NoWait = true;
            return Session.QueuePurgeNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueBindAsync(QueueBind bind)
        {
            return Session.QueueBindAsync(this, bind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueUnbindAsync(QueueUnbind unbind)
        {
            return Session.QueueUnbindAsync(this, unbind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ConsumerStartAsync(RabbitMQConsumer consumer)
        {
            return Session.ConsumerStartAsync(consumer);
        }

        private async ValueTask<bool> PublishAllContinuation(PublishAllInfo allInfo, CancellationToken timeout)
        {
            Session.LockEvent.Wait();
            while (true)
            {
                if (timeout.IsCancellationRequested)
                {
                    return false;
                }
                try
                {
                    await Session.PublishAllAsync(allInfo).ConfigureAwait(false);
                    return true;
                }
                catch (Exception e)
                {
                    Debugger.Break();
                    continue;
                }
            }
        }

        public async ValueTask<bool> PublishBatch(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties, ReadOnlyMemory<byte>[] messages)
        {
            if (IsClosed)
            {
                return false;
            }

            Session.LockEvent.Wait();
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(60, messages[1].Length, ref properties);
            var batch = new PublishAllInfo[messages.Length];
            for (int i = 0; i < messages.Length; i++)
            {
                batch[i] = new PublishAllInfo(ChannelId, ref messages[i], ref info, content);
            }

            await Session.PublishBatchAsync(batch);
            return true;

        }
        public async ValueTask<bool> Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties, ReadOnlyMemory<byte> message)
        {
            if (IsClosed)
            {
                return false;
            }
            Session.LockEvent.Wait();
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(60, message.Length, ref properties);
            if (message.Length <= Session.Tune.FrameMax)
            {
                var allinfo = new PublishAllInfo(ChannelId, ref message, ref info, content);
                try
                {
                    await Session.PublishAllAsync(allinfo).ConfigureAwait(false);
                    return true;
                }
                catch (Exception e)
                {
                    Debugger.Break();
                    var cts = new CancellationTokenSource(Session.Options.ConnectionTimeout);
                    using (var timeoutRegistratiuon = cts.Token.Register(() => cts.Cancel()))
                    {
                        return await PublishAllContinuation(allinfo, cts.Token);
                    }
                }

            }


            await WriterSemaphore.WaitAsync().ConfigureAwait(false);

            var written = 0;
            var partialInfo = new PublishPartialInfo(ref info, ref content);
            await Session.PublishPartialAsync(ChannelId, partialInfo).ConfigureAwait(false);

            while (written < content.BodySize)
            {
                var batchCnt = 0;
                while (batchCnt < _publishBatchSize && written < content.BodySize)
                {
                    var writable = Math.Min(Session.Tune.FrameMax, (int)content.BodySize - written);
                    _publishBatch[batchCnt] = message.Slice(written, writable);
                    batchCnt++;
                    written += writable;
                }
                await Session.PublishBodyAsync(ChannelId, _publishBatch).ConfigureAwait(false);

                _publishBatch.AsSpan().Fill(ReadOnlyMemory<byte>.Empty);
            }

            WriterSemaphore.Release();
            return true;
        }

        public ValueTask Ack(AckInfo ack)
        {
            return Session.Writer.SendAckAsync(ChannelId, ref ack);
        }

        public ValueTask Reject(RejectInfo reject)
        {
            //if (IsClosed)
            //{
            //    throw new Exception($"{nameof(RabbitMQChannel)}.{nameof(Reject)}: channel is canceled");
            //}
            return Session.Writer.SendRejectAsync(ChannelId, ref reject);
        }

        public Task QoS(QoSInfo qos)
        {
            return Session.QoS(this, qos);
        }
    }
}