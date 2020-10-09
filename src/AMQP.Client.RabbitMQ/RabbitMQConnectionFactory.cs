using System;
using System.Net;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnectionFactory
    {
        private readonly RabbitMQConnectionFactoryBuilder _builder;

        internal RabbitMQConnectionFactory(RabbitMQConnectionFactoryBuilder builder)
        {
            _builder = builder;
        }

        public RabbitMQConnection CreateConnection()
        {
            return new RabbitMQConnection(_builder);
        }

        public static RabbitMQConnectionFactory Create(EndPoint endpoint, Action<RabbitMQConnectionFactoryBuilder> configure)
        {
            var buidler = new RabbitMQConnectionFactoryBuilder(endpoint);
            configure(buidler);
            return new RabbitMQConnectionFactory(buidler);
        }
    }
}