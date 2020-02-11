using AMQP.Client.RabbitMQ.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Exchange
{
    public class ExchangeBuilder
    {
        private readonly short _channelId;
        private readonly ExchangeHandler _handler;
        internal ExchangeBuilder(short channelId, ExchangeHandler handler)
        {
            _channelId = channelId;
            _handler = handler;
        }
        public ExchangeDeclareBuilder AsCreate(string name, string type)
        {
            return new ExchangeDeclareBuilder(_handler,_channelId, name, type);
        }
        public void AsDelete()
        {

        }
    }
}
