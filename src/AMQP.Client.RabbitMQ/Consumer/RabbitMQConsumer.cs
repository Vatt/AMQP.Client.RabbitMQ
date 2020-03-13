using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
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
        private readonly BodyFrameChunkedReader _reader;
        private byte[] _activeDeliver;
        private int _deliverPosition;
        public event Action<DeliverArgs> Received;
        private readonly PipeScheduler _scheduler;

        internal RabbitMQConsumer(string consumerTag, ushort channelId, RabbitMQProtocol protocol, PipeScheduler scheduler)
            : base(consumerTag, channelId, protocol)
        {
            _reader = new BodyFrameChunkedReader(channelId);
            _scheduler = scheduler;
            _deliverPosition = 0;
        }

        internal override async ValueTask ProcessBodyMessage(RabbitMQDeliver deliver)
        {
            _deliverPosition = 0;
            _activeDeliver = ArrayPool<byte>.Shared.Rent((int)deliver.Header.BodySize);
            _reader.Restart(deliver.Header.BodySize);
         
            while (!_reader.IsComplete)
            {                
                var result = await _protocol.Reader.ReadAsync(_reader).ConfigureAwait(false);
                Copy(result.Message);
                _protocol.Reader.Advance();
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
                    Received?.Invoke(arg);
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

    public struct DeliverArgs : IDisposable
    {
        public RabbitMQDeliver DeliverInfo { get; }
        public ReadOnlySpan<byte> Body => new ReadOnlySpan<byte>(_body, 0, (int)DeliverInfo.Header.BodySize);
        private byte[] _body;

        public ContentHeaderProperties Properties => DeliverInfo.Header.Properties;

        internal DeliverArgs(ref RabbitMQDeliver deliverInfo, byte[] body)
        {
            DeliverInfo = deliverInfo;
            _body = body;
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
