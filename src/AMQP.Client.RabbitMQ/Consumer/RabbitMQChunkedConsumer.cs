using AMQP.Client.RabbitMQ.Channel;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{

    public class RabbitMQChunkedConsumer : ConsumerBase
    {
        private readonly BodyFrameChunkedReader _reader;

        public event Action<RabbitMQDeliver, ChunkedConsumeResult> Received;
        internal RabbitMQChunkedConsumer(string consumerTag, ConsumerInfo info, RabbitMQProtocol protocol, RabbitMQChannel channel)
            :base(consumerTag, info, protocol, channel)
        {
            _reader = new BodyFrameChunkedReader(channel.ChannelId);
        }

        internal override async ValueTask ProcessBodyMessage(RabbitMQDeliver deliver)
        {
            _reader.Restart(deliver.Header.BodySize);
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
