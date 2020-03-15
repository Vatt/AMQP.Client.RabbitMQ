using System;
using System.Net;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Protocol.Framing;

namespace ProducerTest
{
    class Program
    {
        private const string Host = "centos0.mshome.net";
        static async Task Main(string[] args)
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();

            var properties = ContentHeaderProperties.Default();
            properties.AppId = "testapp";
            var body = new byte[32];
            while (!channel.IsClosed)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                //await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
                await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
            }
        }
    }
}
