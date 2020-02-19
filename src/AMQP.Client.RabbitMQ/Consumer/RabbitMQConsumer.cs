using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public class RabbitMQConsumer : ConsumerBase
    {
        private readonly RabbitMQProtocol _protocol;
        private readonly BodyFrameReader _reader;
        public event Action<ContentHeader, byte[]> Received;
        public event Action Close;
        internal RabbitMQConsumer(string consumerTag, RabbitMQProtocol protocol,ushort channelId) : base(consumerTag, channelId)
        {
            _protocol = protocol;
            _reader = new BodyFrameReader();
        }
        internal override async ValueTask Delivery(DeliverInfo info)
        {
            //добавить чтение ContentHeader
            /*var headerResult = await _protocol.Reader.ReadAsync(new FrameHeaderReader());
            _protocol.Reader.Advance();
            byte[] buffer = ArrayPool<byte>.Shared.Rent((int)header.BodySize);
            _reader.Reset(headerResult.Message, buffer);
            await _protocol.Reader.ReadAsync(_reader);
            _protocol.Reader.Advance();
            Received?.Invoke(header, buffer);            
            ArrayPool<byte>.Shared.Return(buffer);
            */
            throw new NotImplementedException();
        }
    }
}
