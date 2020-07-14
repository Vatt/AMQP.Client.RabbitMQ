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
    }
}