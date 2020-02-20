using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Exchange;
using AMQP.Client.RabbitMQ.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
namespace Test
{

    class Program
    {        

        static async Task Main(string[] args)
        {
            var size = Unsafe.SizeOf<RabbitMQDeliver>();
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("gamover", "gam2106", "/")
                                    .Heartbeat(60*10)
                                    .ProductName("AMQP.Client.RabbitMQ")
                                    .ProductVersion("0.0.1")
                                    .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                    .ClientInformation("TEST TEST TEST")
                                    .ClientCopyright("©")
                                    .Build();

            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });

            var queueOk = await channel.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");
            var consumer = await channel.CreateChunkedConsumer("TestQueue", "TestConsumer",noAck:false);
            //var consumer = await channel.CreateConsumer("TestQueue", "TestConsumer",noAck:false);
            long chunkLen = 0;
            consumer.Received += async (deliver, result) =>
            {
                chunkLen += result.Chunk.Length;

                if (result.IsCompleted)
                {
                    if (chunkLen != 32)
                    {
                        throw new Exception($"Wrong consume length: {chunkLen}");
                    }
                    chunkLen = 0;
                    await deliver.Ack(true);
                }
                
            };
            await connection.WaitEndReading();//for testing
        }

    }

}
