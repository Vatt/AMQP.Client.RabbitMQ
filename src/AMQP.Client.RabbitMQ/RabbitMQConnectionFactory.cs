using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnectionFactory
    {
        private RabbitMQConnectionFactoryBuilder _builder;
        internal RabbitMQConnectionFactory(RabbitMQConnectionFactoryBuilder builder)
        {
            _builder = builder;
        }
        public RabbitMQConnection CreateConnection()
        {
            return new RabbitMQConnection(_builder);
        }
    }
}
