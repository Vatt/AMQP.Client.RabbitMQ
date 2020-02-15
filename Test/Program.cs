using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Exchange;
using AMQP.Client.RabbitMQ.Protocol;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
namespace Test
{

    class Program
    {        

        static async Task Main(string[] args)
        {
            var size = Unsafe.SizeOf<ChunkedConsumeResult>();
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("gamover", "gam2106", "/")
                                    .Heartbeat(120)
                                    .ProductName("AMQP.Client.RabbitMQ")
                                    .ProductVersion("0.0.1")
                                    .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                    .ClientInformation("TEST TEST TEST")
                                    .ClientCopyright("©")
                                    .Build();

            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, true, true, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });

            var queueOk = await channel.QueueDeclareAsync("TestQueue", false, true, true, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");
            await channel.CreateChunkedConsumer("TestQueue", "TestConsumer");
            await connection.WaitEndReading();//for testing
        }

    }

}
