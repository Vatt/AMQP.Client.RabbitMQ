using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ProducerTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionFactoryBuilder builder = new RabbitMQConnectionFactoryBuilder(new IPEndPoint(address, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Heartbeat(60 * 10)
                                 .ProductName("AMQP.Client.RabbitMQ")
                                 .ProductVersion("0.0.1")
                                 .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                 .ClientInformation("TEST TEST TEST")
                                 .ClientCopyright("©")
                                 .Build();
            var connection = factory.MakeNew();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();

            var properties = ContentHeaderProperties.Default();
            properties.AppId("testapp");
            //var body = new byte[16 * 1024 * 1024];
            var body = new byte[32];
            while (!channel.IsClosed)
            {
                properties.CorrelationId(Guid.NewGuid().ToString());
                //await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
                await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
            }
        }
    }
}
