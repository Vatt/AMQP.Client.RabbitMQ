using AMQP.Client.RabbitMQ;
using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;

namespace ConsumerTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            RabbitMQConnectionFactoryBuilder builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint("centos2.mshome.net", 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Heartbeat(60 * 10)
                                 .ProductName("AMQP.Client.RabbitMQ")
                                 .ProductVersion("0.0.1")
                                 .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                 .ClientInformation("TEST TEST TEST")
                                 .ClientCopyright("©")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            var consumer = channel.CreateConsumer("TestQueue", "TestConsumer", PipeScheduler.ThreadPool, noAck: true);           
            consumer.Received += (result) =>
            {
                //await channel.Ack(deliver.DeliveryTag);
            };
            await connection.WaitEndReading();
            await consumer.ConsumerStartAsync();
        }
    }
}
