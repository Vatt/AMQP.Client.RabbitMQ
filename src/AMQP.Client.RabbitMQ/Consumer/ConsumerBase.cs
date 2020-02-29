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
        public readonly string ConsumerTag;
        public readonly ushort ChannelId;
        protected readonly RabbitMQProtocol _protocol;
        internal TaskCompletionSource<string> CancelSrc;
        private SemaphoreSlim _semaphore;
        public bool IsCanceled { get; protected set; }
        internal ConsumerBase(string tag, ushort channel, RabbitMQProtocol protocol)
        {
            ConsumerTag = tag;
            ChannelId = channel;
            _protocol = protocol;
            IsCanceled = false;
            _semaphore = new SemaphoreSlim(1);
        }
        public async ValueTask<string> CancelAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);            
            CancelSrc = new TaskCompletionSource<string>();            
            await _protocol.Writer.WriteAsync(new BasicConsumeCancelWriter(ChannelId), new ConsumeCancelInfo(ConsumerTag, false)).ConfigureAwait(false);
            var result = await CancelSrc.Task.ConfigureAwait(false);
            IsCanceled = true;
            _semaphore.Release();
            return result;
        }
        internal void SimpleCancel()
        {
            IsCanceled = true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal async ValueTask Delivery(DeliverInfo info)
        {
            if (IsCanceled)
            {
                throw new Exception("Consumer already canceled");
            }
            var contentResult = await _protocol.Reader.ReadAsync(new ContentHeaderFullReader(ChannelId)).ConfigureAwait(false);
            _protocol.Reader.Advance();
            await ProcessBodyMessage(new RabbitMQDeliver(info.DeliverTag, contentResult.Message)).ConfigureAwait(false);
        }
        internal abstract ValueTask ProcessBodyMessage(RabbitMQDeliver deliver);

    }
}
