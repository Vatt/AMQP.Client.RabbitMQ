using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Exchange;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Publisher;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace Test
{

    class Program
    {        

        static async Task Main(string[] args)
        {


            //Utf8JsonWriter
            //Utf8JsonReader
            //JsonSerializer 
            await RunDefault();
            
            //Task.WaitAll(Task.Run(StartConsumer),
            //             Task.Run(StartPublisher));
        }
        private static async Task RunDefault()
        {
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
    
            var publisher = channel.CreatePublisher();
            ContentHeaderProperties properties = new ContentHeaderProperties();
            properties.AppId = "testapp";
            properties.CorrelationId = Guid.NewGuid().ToString();
            for (var i = 0; i < 500000; i++)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                await publisher.Publish("TestExchange", string.Empty, false, false, properties, new byte[32]);
            }

            var consumer = await channel.CreateConsumer("TestQueue", "TestConsumer",noAck:true);
            consumer.Received += async (deliver, result) =>
            {
                //await deliver.Ack();
            };
            await connection.WaitEndReading();
        }
        private static async Task StartPublisher()
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

            var publisher = channel.CreatePublisher();
            ContentHeaderProperties properties = new ContentHeaderProperties();
            properties.AppId = "testapp";
            properties.CorrelationId = Guid.NewGuid().ToString();
            while(true)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                await publisher.Publish("TestExchange", string.Empty, false, false, properties, new byte[32*1024]);
            }
            
        }
        private static async Task StartConsumer()
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
            var consumer = await channel.CreateChunkedConsumer("TestQueue", "TestConsumer", noAck: true);
            consumer.Received +=  (deliver, result) =>
            {
                //await deliver.Ack();
            };
            await connection.WaitEndReading();
        }

    }

}
