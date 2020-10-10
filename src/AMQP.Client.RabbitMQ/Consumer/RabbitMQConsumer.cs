using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Core;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public class DeliverArgs : EventArgs, IDisposable
    {
        public ref ContentHeaderProperties Properties => ref _header.Properties;
        public long DeliveryTag { get; }
        private byte[] _body;
        private ContentHeader _header;
        private int _bodySize;

        public ReadOnlySpan<byte> Body => new ReadOnlySpan<byte>(_body, 0, _bodySize);

        internal DeliverArgs(long deliveryTag, ContentHeader header, byte[] body)
        {
            DeliveryTag = deliveryTag;
            _body = body;
            _bodySize = (int)header.BodySize;
            _header = header;
        }

        public void Dispose()
        {
            if (_body != null)
            {
                ArrayPool<byte>.Shared.Return(_body);
                _body = null;
            }
        }
    }
    public class RabbitMQConsumer : IConsumable
    {

        public event EventHandler<DeliverArgs> Received;
        public event EventHandler<EventArgs> Canceled;
        private readonly PipeScheduler _scheduler;
        private BodyFrameChunkedReader _bodyReader;
        private ContentHeaderFullReader _contentFullReader;
        public RabbitMQChannel Channel;
        private byte[] _activeDeliverBody;
        private ConsumeConf _consume;
        private int _deliverPosition;
        public bool IsClosed { get; internal set; }
        public ref ConsumeConf Conf => ref _consume;
        public RabbitMQConsumer(RabbitMQChannel channel, ConsumeConf conf, PipeScheduler scheduler)
        {
            _consume = conf;
            _scheduler = scheduler;
            Channel = channel;
            _bodyReader = new BodyFrameChunkedReader(Channel.ChannelId);
            _contentFullReader = new ContentHeaderFullReader(Channel.ChannelId);
        }
        public RabbitMQConsumer(RabbitMQChannel channel, ConsumeConf conf)
        {
            _consume = conf;
            _scheduler = PipeScheduler.Inline;
            Channel = channel;
            _bodyReader = new BodyFrameChunkedReader(Channel.ChannelId);
            _contentFullReader = new ContentHeaderFullReader(Channel.ChannelId);
        }
        private void Invoke(object obj)
        {
            if (obj is DeliverArgs arg)
            {
                try
                {
                    Received?.Invoke(this, arg);
                }
                catch (Exception e)
                {
                    // add logger
                    Debugger.Break();

                }
                finally
                {
                    arg.Dispose();
                }

            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Copy(ReadOnlySequence<byte> message)
        {
            var span = new Span<byte>(_activeDeliverBody, _deliverPosition, (int)message.Length);
            message.CopyTo(span);
            _deliverPosition += (int)message.Length;
        }
        public async ValueTask OnBeginDeliveryAsync(RabbitMQDeliver deliver, ProtocolReader protocol)
        {
            var activeContent = await protocol.ReadAsync(_contentFullReader).ConfigureAwait(false);
            protocol.Advance();
            if (activeContent.IsCanceled || activeContent.IsCompleted)
            {
                //TODO: do some
            }
            _activeDeliverBody = ArrayPool<byte>.Shared.Rent((int)activeContent.Message.BodySize);
            _deliverPosition = 0;
            _bodyReader.Reset(activeContent.Message.BodySize);

            while (!_bodyReader.IsComplete)
            {
                var result = await protocol.ReadAsync(_bodyReader).ConfigureAwait(false);
                if (result.IsCanceled || result.IsCompleted)
                {
                    //TODO: do some
                }
                Copy(result.Message);
                protocol.Advance();
            }

            var arg = new DeliverArgs(deliver.DeliverTag, activeContent.Message, _activeDeliverBody);
            _scheduler.Schedule(Invoke, arg);

        }

        public ValueTask OnConsumerCancelAsync()
        {
            IsClosed = true;
            Canceled?.Invoke(this, default);
            return default;
        }
    }
}
