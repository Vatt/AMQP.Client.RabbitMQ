using AMQP.Client.RabbitMQ.Protocol;
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
        private readonly IMessageReader<ReadOnlySequence<byte>> _reader;

        public event Action<DeliverInfo,ChunkedConsumeResult> Received;
        public event Action Close;
        internal RabbitMQChunkedConsumer(string consumerTag, RabbitMQProtocol protocol):base(consumerTag)
        {
            _protocol = protocol;
        }
        internal override void Delivery(DeliverInfo deliver)
        {

        }
        private async ValueTask ReadBody()
        {
            long size = 0;
            long readed = 0;
            while(readed < size)
            {
                var result = await _protocol.Reader.ReadAsync(_reader);
                readed += result.Message.Length;
                var chunk = new ChunkedConsumeResult(result.Message, true);

                Received?.Invoke(new DeliverInfo(), chunk);
                _protocol.Reader.Advance();
            }

        }


    }
}
