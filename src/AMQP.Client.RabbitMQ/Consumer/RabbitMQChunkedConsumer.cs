namespace AMQP.Client.RabbitMQ.Consumer
{
    /*
    public class RabbitMQChunkedConsumer : ConsumerBase
    {
        private readonly IChunkedBodyFrameReader _reader;

        public event Action<RabbitMQDeliver, ChunkedConsumeResult> Received;
        internal RabbitMQChunkedConsumer(ConsumerInfo info, RabbitMQProtocolWriter protocol, RabbitMQChannel channel)
            : base(info, protocol, channel)
        {
            _reader = protocol.CreateResetableChunkedBodyReader(channel.ChannelId);
        }

        internal override async ValueTask ProcessBodyMessage(RabbitMQDeliver deliver)
        {
            _reader.Reset(deliver.Header.BodySize);
            while (!_reader.IsComplete)
            {
                var result = await _protocol.ReadAsync(_reader).ConfigureAwait(false);
                var chunk = new ChunkedConsumeResult(result, _reader.IsComplete);
                Received?.Invoke(deliver, chunk);
            }
        }
    }
    */
}
