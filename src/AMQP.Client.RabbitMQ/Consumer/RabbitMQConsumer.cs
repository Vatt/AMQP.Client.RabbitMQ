using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
        private readonly PipeScheduler _scheduler;
        private BodyFrameChunkedReader _bodyReader;
        private ContentHeaderFullReader _contentFullReader;
        public RabbitMQChannel Channel;
        private byte[] _activeDeliverBody;
        private ConsumeConf _consume;
        private int _deliverPosition;
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
        public async ValueTask OnBeginDeliveryAsync(RabbitMQDeliver deliver, RabbitMQProtocolReader protocol)
        {
            var activeContent = await protocol.ReadAsync(_contentFullReader).ConfigureAwait(false);
            _activeDeliverBody = ArrayPool<byte>.Shared.Rent((int)activeContent.BodySize);
            _deliverPosition = 0;
            _bodyReader.Reset(activeContent.BodySize);

            while (!_bodyReader.IsComplete)
            {
                var result = await protocol.ReadWithoutAdvanceAsync(_bodyReader).ConfigureAwait(false);
                Copy(result);
                protocol.Advance();
            }

            var arg = new DeliverArgs(deliver.DeliverTag, activeContent, _activeDeliverBody);
            _scheduler.Schedule(Invoke, arg);

        }
    }
}
