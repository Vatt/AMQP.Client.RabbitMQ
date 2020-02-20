using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public readonly struct RabbitMQDeliver
    {
        public readonly DeliverInfo Info;
        private readonly RabbitMQProtocol _protocol;
        private readonly ushort _channelId;
        public RabbitMQDeliver(DeliverInfo info, ushort channelId, RabbitMQProtocol protocol)
        {
            Info = info;
            _protocol = protocol;
            _channelId = channelId;
        }
        public async ValueTask Ack(bool multiple)
        {
            await _protocol.Writer.WriteAsync(new BasicAckWriter(_channelId), new AckInfo(Info.DeliverTag, multiple));
        }
        public async ValueTask Reject()
        {

        }



    }
}
