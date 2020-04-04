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
        public ContentHeaderProperties Properties { get; }
        public long DeliveryTag { get; }
        private byte[] _body;
        private int _bodySize;

        public ReadOnlySpan<byte> Body => new ReadOnlySpan<byte>(_body, 0, _bodySize);

        internal DeliverArgs(long deliveryTag, ref ContentHeader header, byte[] body)
        {
            Properties = header.Properties;
            DeliveryTag = deliveryTag;
            _body = body;
            _bodySize = (int)header.BodySize;
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

        private IChunkedBodyFrameReader _reader;
        public event EventHandler<DeliverArgs> Received;
        private readonly PipeScheduler _scheduler;
        public ConsumeConf Consume;
        public RabbitMQChannel Channel;
        public RabbitMQConsumer(RabbitMQChannel channel, ConsumeConf conf, PipeScheduler scheduler)
        {
            Consume = conf;
            _scheduler = scheduler;
            Channel = channel;
        }
        public RabbitMQConsumer(RabbitMQChannel channel, ConsumeConf conf)
        {
            Consume = conf;
            _scheduler = PipeScheduler.Inline;
            Channel = channel;
        }

        public async ValueTask OnDeliveryAsync(Deliver deliver, RabbitMQProtocolReader protocol)
        {
            //if (IsCanceled)
            //{
            //    throw new Exception("Consumer already canceled");
            //}
            var content = await protocol.ReadContentHeaderWithFrameHeaderAsync(Channel.ChannelId).ConfigureAwait(false);
            protocol.Advance();
            var _deliverPosition = 0;
            var _activeDeliver = ArrayPool<byte>.Shared.Rent((int)content.BodySize);
            _reader.Reset(content.BodySize);

            while (!_reader.IsComplete)
            {
                var result = await protocol.ReadWithoutAdvanceAsync(_reader).ConfigureAwait(false);
                Copy(result, ref _activeDeliver, ref _deliverPosition);
                protocol.Advance();
            }

            var arg = new DeliverArgs(deliver.DeliverTag, ref content, _activeDeliver);
            _scheduler.Schedule(Invoke, arg);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Copy(ReadOnlySequence<byte> message, ref byte[] activeDeliver, ref int deliverPosition)
        {
            var span = new Span<byte>(activeDeliver, deliverPosition, (int)message.Length);
            message.CopyTo(span);
            deliverPosition += (int)message.Length;
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
    }
}
