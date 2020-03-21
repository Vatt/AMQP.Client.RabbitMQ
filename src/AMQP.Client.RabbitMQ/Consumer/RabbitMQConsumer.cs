using AMQP.Client.RabbitMQ.Channel;
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
    public class RabbitMQConsumer : ConsumerBase
    {
        private readonly IChunkedBodyFrameReader _reader;
        private byte[] _activeDeliver;
        private int _deliverPosition;
        public event EventHandler<DeliverArgs> Received;
        private readonly PipeScheduler _scheduler;

        internal RabbitMQConsumer(ConsumerInfo info, RabbitMQProtocol protocol, PipeScheduler scheduler, RabbitMQChannel channel)
            : base(info, protocol, channel)
        {
            _reader = protocol.CreateResetableChunkedBodyReader(channel.ChannelId);
            _scheduler = scheduler;
            _deliverPosition = 0;
        }

        internal override async ValueTask ProcessBodyMessage(RabbitMQDeliver deliver)
        {
            _deliverPosition = 0;
            _activeDeliver = ArrayPool<byte>.Shared.Rent((int)deliver.Header.BodySize);
            _reader.Reset(deliver.Header.BodySize);

            while (!_reader.IsComplete)
            {
                var result = await _protocol.ReadWithoutAdvanceAsync(_reader).ConfigureAwait(false);
                Copy(result);
                _protocol.ReaderAdvance();
            }

            var arg = new DeliverArgs(ref deliver, _activeDeliver);
            _scheduler.Schedule(Invoke, arg);
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
            var span = new Span<byte>(_activeDeliver, _deliverPosition, (int)message.Length);
            message.CopyTo(span);
            _deliverPosition += (int)message.Length;
        }
    }

    public class DeliverArgs : EventArgs, IDisposable
    {
        public ContentHeaderProperties Properties { get; }
        public long DeliveryTag { get; }
        private byte[] _body;
        private int _bodySize;

        public ReadOnlySpan<byte> Body => new ReadOnlySpan<byte>(_body, 0, _bodySize);

        internal DeliverArgs(ref RabbitMQDeliver deliverInfo, byte[] body)
        {
            Properties = deliverInfo.Header.Properties;
            DeliveryTag = deliverInfo.DeliveryTag;
            _body = body;
            _bodySize = (int)deliverInfo.Header.BodySize;
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
}
