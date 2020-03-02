using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Handlers;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
namespace Test
{

    class Program
    {        

        static async Task Main(string[] args)
        {
            //using Microsoft.Extensions.ObjectPool;
            //private static ObjectPool<FrameContentReader> _readerPool = ObjectPool.Create<FrameContentReader>();
            //запилить пропертисы в сраный словарь

            //Utf8JsonWriter
            //Utf8JsonReader
            //JsonSerializer 

            await RunNothing();
            //await RunDefault();
            //await ChannelTest();
            //Task.WaitAny(Task.Run(StartConsumer),
            //             Task.Run(StartPublisher));
        }
        public static async Task ChannelTest()
        {
            var addresses = Dns.GetHostAddresses("centos0.mshome.net");
            var address = addresses.First();
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("guest", "guest", "/")
                                    .Heartbeat(60)
                                    .ProductName("AMQP.Client.RabbitMQ")
                                    .ProductVersion("0.0.1")
                                    .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                    .ClientInformation("TEST TEST TEST")
                                    .ClientCopyright("©")
                                    .Build();
            await connection.StartAsync();
            var channel1 = await connection.CreateChannel();
            var channel2 = await connection.CreateChannel();
            await channel1.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            var queueOk1 = await channel1.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel1.QueueBindAsync("TestQueue", "TestExchange");


            await channel2.ExchangeDeclareAsync("TestExchange2", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            var queueOk2 = await channel2.QueueDeclareAsync("TestQueue2", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel2.QueueBindAsync("TestQueue2", "TestExchange2");

            await channel1.QoS(0, 10, false);
            await channel2.QoS(0, 10, false);

            var body1 = new byte[1024];
            body1.AsSpan(0, 512).Fill(69);
            body1.AsSpan(512).Fill(42);

            var body2 = new byte[2048];
            body2.AsSpan(0, 1024).Fill(96);
            body2.AsSpan(1024).Fill(24);


            //var publisher1 = channel1.CreatePublisher();
            //var publisher2 = channel2.CreatePublisher();

            
            var consumer1 = await channel1.CreateConsumer("TestQueue", "TestConsumer", noAck: true);
            consumer1.Received += async (deliver, result) =>
            {
                if (result.Length != 1024 || result[0] != 69 || result[1024 - 1] != 42)
                {
                    Debugger.Break();
                }
                //await channel1.Ack(deliver.DeliveryTag, true);
                var propertiesConsume = ContentHeaderProperties.Default();
                propertiesConsume.AppId( "testapp2" );
                await channel2.Publish("TestExchange2", string.Empty, false, false, propertiesConsume, body2);
                
            };

            var consumer2 = await channel2.CreateConsumer("TestQueue2", "TestConsumer2", noAck: true);
            consumer2.Received += async (deliver, result) =>
            {
                if (result.Length != 2048 || result[0] != 96 || result[2048 - 1] != 24)
                {
                    Debugger.Break();
                }
                //await channel2.Ack(deliver.DeliveryTag, true);
                var propertiesConsume = ContentHeaderProperties.Default();
                propertiesConsume.AppId("testapp1" );
                await channel1.Publish("TestExchange", string.Empty, false, false, propertiesConsume, body1);                
            };
            
            var firtsTask = Task.Run(async () =>
            {
                var properties = ContentHeaderProperties.Default();
                properties.AppId( "testapp1" );
                while (channel1.IsOpen)
                {
                    await channel1.Publish("TestExchange", string.Empty, false, false, properties, body1);
                }
            });
            var secondTask = Task.Run(async () =>
            {
                var properties = ContentHeaderProperties.Default();
                properties.AppId( "testapp2" );
                while (channel2.IsOpen)
                {
                    await channel2.Publish("TestExchange2", string.Empty, false, false, properties, body2);
                }
            });

            await connection.WaitEndReading();
        }
        private static async Task RunDefault()
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("guest", "guest", "/")
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
    
            var properties = ContentHeaderProperties.Default();
            properties.AppId( "testapp" );
            for (var i = 0; i < 40000; i++)
            {
                properties.CorrelationId( Guid.NewGuid().ToString() );
                await channel.Publish("TestExchange", string.Empty, false, false, properties, new byte[32]);
            }

            var consumer = await channel.CreateConsumer("TestQueue", "TestConsumer",noAck:true);
            consumer.Received += async (deliver, result) =>
            {
               // await channel.Ack(deliver.DeliveryTag);
            };
            await connection.WaitEndReading();
        }
        private static async Task StartPublisher()
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("guest", "guest", "/")
                        .Heartbeat(60)
                        .ProductName("AMQP.Client.RabbitMQ")
                        .ProductVersion("0.0.1")
                        .ConnectionName("AMQP.Client.RabbitMQ:Test")
                        .ClientInformation("TEST TEST TEST")
                        .ClientCopyright("©")
                        .Build();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            var queueOk1 = await channel.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");


            await channel.ExchangeDeclareAsync("TestExchange2", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            var queueOk2 = await channel.QueueDeclareAsync("TestQueue2", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue2", "TestExchange2");

            var properties = ContentHeaderProperties.Default();
            properties.AppId( "testapp" );
            //var body = new byte[16 * 1024 * 1024 + 1];
            while (channel.IsOpen)
            {
                properties.CorrelationId( Guid.NewGuid().ToString() );
                //await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
                await channel.Publish("TestExchange", string.Empty, false, false, properties, new byte[32]);
            }
            await connection.WaitEndReading();
            
        }
        private static async Task StartConsumer()
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("guest", "guest", "/")
                        .Heartbeat(60)
                        .ProductName("AMQP.Client.RabbitMQ")
                        .ProductVersion("0.0.1")
                        .ConnectionName("AMQP.Client.RabbitMQ:Test")
                        .ClientInformation("TEST TEST TEST")
                        .ClientCopyright("©")
                        .Build();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();

            //await channel.QoS(0, ushort.MaxValue, true);

            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            var queueOk1 = await channel.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");


            await channel.ExchangeDeclareAsync("TestExchange2", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            var queueOk2 = await channel.QueueDeclareAsync("TestQueue2", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue2", "TestExchange2");

            //var consumer = await channel.CreateChunkedConsumer("TestQueue", "TestConsumer", noAck: false);
            //consumer.Received += async (deliver, result) =>
            //{

            //    if(result.IsCompleted)
            //    {
            //        await channel.Ack(deliver.DeliveryTag, false);                    
            //    }

            //};
            var consumer = await channel.CreateChunkedConsumer("TestQueue", "TestConsumer", noAck: true);
            consumer.Received += async (deliver, result) =>
            {                
                //await channel.Ack(deliver.DeliveryTag, false);
            };
            //await channel.CloseChannelAsync("kek");
            await connection.WaitEndReading();
        }

        private static async Task RunNothing()
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("guest", "guest", "/")
                        .Heartbeat(60)
                        .ProductName("AMQP.Client.RabbitMQ")
                        .ProductVersion("0.0.1")
                        .ConnectionName("AMQP.Client.RabbitMQ:Test")
                        .ClientInformation("TEST TEST TEST")
                        .ClientCopyright("©")
                        .Build();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            Action waiter = async () => {
                var info = await channel.WaitClosing();
                Console.WriteLine($"Channel closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
            };
            await channel.CloseChannelAsync("kek");
            waiter();
            await connection.CloseConnection();
            await connection.WaitEndReading();
        }
    }

}
