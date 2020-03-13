using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public class RabbitMQConsumer : ConsumerBase
    {
        private readonly BodyFrameChunkedReader _reader;
        private byte[] _activeDeliver;
        private int _deliverPosition;
        public event Action<RabbitMQDeliver, byte[]> Received;

        internal RabbitMQConsumer(string consumerTag, ushort channelId, RabbitMQProtocol protocol)
            : base(consumerTag, channelId, protocol)
        {
            _reader = new BodyFrameChunkedReader(channelId);
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
                Copy(result.Message.Slice(0));//TODO: WTF?!
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
    }
}
