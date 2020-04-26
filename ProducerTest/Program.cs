using System;
using System.Net;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;

namespace ProducerTest
{
    internal class Program
    {
        private const string Host = "centos0.mshome.net";

        private static async Task Main(string[] args)
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.Build();
            var connection = factory.CreateConnection();

            await connection.StartAsync();

            var channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create("TestExchange", ExchangeType.Direct));
            await channel.QueueDeclareAsync(QueueDeclare.Create("TestQueue"));
            await channel.QueueBindAsync(QueueBind.Create("TestQueue", "TestExchange"));

            var properties = new ContentHeaderProperties();
            properties.AppId = "testapp";
            //var body = new byte[16 * 1024 * 1024 + 1];
            var body = new byte[32];
            //var body = new byte[16*1024];
            //var body = new byte[128*1024];
            //var body = new byte[512 * 1024];
            //var body = new byte[1 * 1024 * 1024];
            //var body = new byte[1024];

            while (true /*!channel.IsClosed*/)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
            }

            //await Task.Delay(TimeSpan.FromHours(1));
        }
    }
}