using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Internal;

namespace AMQP.Client.RabbitMQ
{
    public sealed class RabbitMQChannel : ChannelData
    {
        private readonly (ushort, ReadOnlyMemory<byte>)[] _publishBatch;
        private static readonly int _publishBatchSize = 4;
        public readonly ushort ChannelId;

        internal RabbitMQChannel(ushort id, RabbitMQSession session) : base(session)
        {
            ChannelId = id;
            _publishBatch = new (ushort, ReadOnlyMemory<byte>)[_publishBatchSize];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ExchangeDeclareAsync(ExchangeDeclare exchange)
        {
            ThrowIfConnectionClosed();
            return Session.ExchangeDeclareAsync(this, exchange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask ExchangeDeleteAsync(ExchangeDelete exchange)
        {
            ThrowIfConnectionClosed();
            return Session.ExchangeDeleteAsync(this, exchange);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<QueueDeclareOk> QueueDeclareAsync(QueueDeclare queue)
        {
            ThrowIfConnectionClosed();
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
            ThrowIfConnectionClosed();
            return Session.QueueDeleteAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueDeleteNoWaitAsync(QueueDelete queue)
        {
            ThrowIfConnectionClosed();
            queue.NoWait = true;
            return Session.QueueDeleteNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> QueuePurgeAsync(QueuePurge queue)
        {
            ThrowIfConnectionClosed();
            return Session.QueuePurgeAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueuePurgeNoWaitAsync(QueuePurge queue)
        {
            ThrowIfConnectionClosed();
            queue.NoWait = true;
            return Session.QueuePurgeNoWaitAsync(this, queue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueBindAsync(QueueBind bind)
        {
            ThrowIfConnectionClosed();
            return Session.QueueBindAsync(this, bind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask QueueUnbindAsync(QueueUnbind unbind)
        {
            ThrowIfConnectionClosed();
            return Session.QueueUnbindAsync(this, unbind);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task ConsumerStartAsync(RabbitMQConsumer consumer)
        {
            ThrowIfConnectionClosed();
            return Session.ConsumerStartAsync(consumer);
        }

        private async ValueTask<bool> PublishAllContinuation(BasicPublishInfo info, ContentHeader content, ReadOnlyMemory<byte> message, CancellationToken timeout)
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
                    await Session.Writer.WriteAsync3(
                            ProtocolWriters.BasicPublishWriter, info,
                            ProtocolWriters.ContentHeaderWriter, content,
                            ProtocolWriters.BodyFrameWriter, (ChannelId, message))
                        .ConfigureAwait(false);
                    return true;
                }
                catch (Exception e)
                {
                    Debugger.Break();
                    return false;
                }
            }
        }

        public async ValueTask<bool> Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties, ReadOnlyMemory<byte> message)
        {
            ThrowIfConnectionClosed();
            Session.LockEvent.Wait();
            var info = new BasicPublishInfo(ChannelId, exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(ChannelId, 60, message.Length, ref properties);
            if (message.Length <= Session.Tune.FrameMax)
            {
                try
                {

                    await Session.Writer.WriteAsync3(
                        ProtocolWriters.BasicPublishWriter, info,
                        ProtocolWriters.ContentHeaderWriter, content,
                        ProtocolWriters.BodyFrameWriter, (ChannelId, message))
                        .ConfigureAwait(false);
                    return true;
                }
                catch (Exception e)
                {
                    Debugger.Break();
                    
                    var cts = new CancellationTokenSource(Session.Options.ConnectionTimeout);
                    using (var timeoutRegistratiuon = cts.Token.Register(() => cts.Cancel()))
                    {
                        Session.SetException(e);
                        return await PublishAllContinuation(info, content, message, cts.Token);
                    }
                }
            }


            await WriterSemaphore.WaitAsync().ConfigureAwait(false);

            var written = 0;
            try
            {
                await Session.Writer.WriteAsync2(
                        ProtocolWriters.BasicPublishWriter, info,
                        ProtocolWriters.ContentHeaderWriter, content)
                    .ConfigureAwait(false);

                while (written < content.BodySize)
                {
                    var batchCnt = 0;
                    while (batchCnt < _publishBatchSize && written < content.BodySize)
                    {
                        var writable = Math.Min(Session.Tune.FrameMax, (int) content.BodySize - written);
                        _publishBatch[batchCnt] = (ChannelId, message.Slice(written, writable));
                        batchCnt++;
                        written += writable;
                    }
                    await Session.Writer.WriteManyAsync(ProtocolWriters.BodyFrameWriter, _publishBatch).ConfigureAwait(false);
                    _publishBatch.AsSpan().Fill((0,ReadOnlyMemory<byte>.Empty));
                }


            }
            catch (Exception ex)
            {
                Session.SetException(ex);
            }
            finally
            {
                WriterSemaphore.Release();
            }
            return true;

        }

        public ValueTask Ack(AckInfo ack)
        {
            ThrowIfConnectionClosed();
            return Session.Writer.WriteAsync(ProtocolWriters.BasicAckWriter, ack);
        }

        public ValueTask Reject(RejectInfo reject)
        {
            ThrowIfConnectionClosed();
            return Session.Writer.WriteAsync(ProtocolWriters.BasicRejectWriter, reject);
        }

        public Task QoS(QoSInfo qos)
        {
            ThrowIfConnectionClosed();
            return Session.QoS(this, qos);
        }

        public async Task CloseAsync()
        {
            await Session.CloseChannel(this, $"Channel {ChannelId} closed gracefully");
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfConnectionClosed()
        {
            if (IsClosed)
            {
                RabbitMQExceptionHelper.ThrowConnectionClosed(ChannelId);
            }
        }
    }
}