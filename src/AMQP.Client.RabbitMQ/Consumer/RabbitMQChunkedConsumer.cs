using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{

    public class RabbitMQChunkedConsumer : ConsumerBase
    {
        private readonly RabbitMQProtocol _protocol;
        private readonly BodyFrameChunkedReader _reader;

        public event Action<ContentHeader, ChunkedConsumeResult> Received;
        public event Action Close;
        internal RabbitMQChunkedConsumer(string consumerTag, RabbitMQProtocol protocol):base(consumerTag)
        {
            _protocol = protocol;
            _reader = new BodyFrameChunkedReader();
        }
        internal override async ValueTask Delivery(DeliverInfo info,ContentHeader header)
        {
            await ReadBody(info,header);
        }
        private async ValueTask ReadBody(DeliverInfo info,ContentHeader header)
        {
            var headerResult = await _protocol.Reader.ReadAsync(new FrameHeaderReader());
            _reader.Restart(headerResult.Message);
            _protocol.Reader.Advance();
            while(_reader.Consumed < header.BodySize)
            {
                var result = await _protocol.Reader.ReadAsync(_reader);
                var chunk = new ChunkedConsumeResult(result.Message, _reader.Consumed == header.BodySize);                
                Received?.Invoke(header, chunk);
                _protocol.Reader.Advance();

            }

        }


    }
}
