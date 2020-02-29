using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public readonly struct RabbitMQDeliver
    {
        public readonly ContentHeader Header;
        public readonly long DeliveryTag;     
        internal RabbitMQDeliver(long deliveryTag, ContentHeader header)
        {
            Header = header;
            DeliveryTag = deliveryTag;
        }
    }
}
