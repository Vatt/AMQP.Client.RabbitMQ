using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public class RabbitMQConsumer : ConsumerBase
    {
        private readonly BodyFrameChunkedReader _reader;
        private byte[] _activeDeliver;
        private int _deliverPosition;
        public event Action<RabbitMQDeliver, byte[]> Received;

        internal RabbitMQConsumer(string consumerTag, RabbitMQProtocol protocol, ushort channelId)
            : base(consumerTag, channelId, protocol)
        {
            _reader = new BodyFrameChunkedReader();
            _deliverPosition = 0;
        }
        internal override async ValueTask ReadBodyMessage(RabbitMQDeliver deliver, ContentHeader header)
        {
            var headerResult = await _protocol.Reader.ReadAsync(new FrameHeaderReader()).ConfigureAwait(false);
            _protocol.Reader.Advance();
            Restart(header);

            while (!_reader.IsComplete)
            {
                var result = await _protocol.Reader.ReadAsync(_reader).ConfigureAwait(false);
                Copy(result.Message);
                _protocol.Reader.Advance();
            }
            Received?.Invoke(deliver, _activeDeliver);
            ArrayPool<byte>.Shared.Return(_activeDeliver);
            _activeDeliver = null;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Copy(ReadOnlySequence<byte> message)
        {
            var span = new Span<byte>(_activeDeliver, _deliverPosition, (int)message.Length);
            message.CopyTo(span);
            _deliverPosition += (int)message.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Restart(ContentHeader header)
        {
            _deliverPosition = 0;
            _activeDeliver = ArrayPool<byte>.Shared.Rent((int)header.BodySize);
            _reader.Restart(header);
        }
    }
}
