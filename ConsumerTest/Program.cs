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
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionFactoryBuilder builder = new RabbitMQConnectionFactoryBuilder(new IPEndPoint(address, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Heartbeat(60 )
                                 .ProductName("AMQP.Client.RabbitMQ")
                                 .ProductVersion("0.0.1")
                                 .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                 .ClientInformation("TEST TEST TEST")
                                 .ClientCopyright("©")
                                 .Build();
            var connection = factory.MakeNew();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            var consumer = await channel.CreateConsumer("TestQueue", "TestConsumer", PipeScheduler.ThreadPool, noAck: true);
            consumer.Received += async (deliver, result) =>
            {
                //await channel.Ack(deliver.DeliveryTag);
            };
            await connection.WaitEndReading();
        }
    }
}
