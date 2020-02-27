using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{

    public class RabbitMQChunkedConsumer : ConsumerBase
    {
        private readonly BodyFrameChunkedReader _reader;

        public event Action<RabbitMQDeliver, ChunkedConsumeResult> Received;
        internal RabbitMQChunkedConsumer(string consumerTag, RabbitMQProtocol protocol, ushort channelId, SemaphoreSlim semaphore)
            :base(consumerTag, channelId, protocol, semaphore)
        {
            _reader = new BodyFrameChunkedReader(channelId);
        }

        internal override async ValueTask ProcessBodyMessage(RabbitMQDeliver deliver, long contentBodySize)
        {
            //var headerResult = await _protocol.Reader.ReadAsync(new FrameHeaderReader()).ConfigureAwait(false);
            //_protocol.Reader.Advance();
            _reader.Restart(contentBodySize);

            while (!_reader.IsComplete)
            {
                var result = await _protocol.Reader.ReadAsync(_reader).ConfigureAwait(false);
                var chunk = new ChunkedConsumeResult(result.Message, _reader.IsComplete);
                Received?.Invoke(deliver, chunk);
                _protocol.Reader.Advance();
            }
        }
    }
}
