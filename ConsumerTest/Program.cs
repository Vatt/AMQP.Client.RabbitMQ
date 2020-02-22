using AMQP.Client.RabbitMQ;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ConsumerTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("gamover", "gam2106", "/")
                        .Heartbeat(60 * 10)
                        .ProductName("AMQP.Client.RabbitMQ")
                        .ProductVersion("0.0.1")
                        .ConnectionName("AMQP.Client.RabbitMQ:Test")
                        .ClientInformation("TEST TEST TEST")
                        .ClientCopyright("©")
                        .Build();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            var consumer = await channel.CreateConsumer("TestQueue", "TestConsumer", noAck: true);
            consumer.Received += async (deliver, result) =>
            {
                //await deliver.Ack();
            };
            await connection.WaitEndReading();
        }
    }
}
