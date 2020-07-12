using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Handlers;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;
namespace Test
{
    class Program
    {
        private static string Host = "centos0.mshome.net";
        private static int Size = 32;

        //private static string Host = 
        static async Task Main(string host, int size)
        {
            //using Microsoft.Extensions.ObjectPool;
            //private static ObjectPool<FrameContentReader> _readerPool = ObjectPool.Create<FrameContentReader>();

            //Utf8JsonWriter
            //Utf8JsonReader
            //JsonSerializer 

            //await RunNothing();
            //await RunDefault();
            //await ChannelTest();


            //await RunDefault();

            if (!string.IsNullOrEmpty(host))
            {
                Host = host;
            }

            if (size > 0)
            {
                Size = size;
            }

            await Task.WhenAll(StartConsumer(), StartPublisher());
        }
        public static async Task ChannelTest()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
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

            var body1 = new byte[Size];


            //var publisher1 = channel1.CreatePublisher();
            //var publisher2 = channel2.CreatePublisher();


            var consumer1 = channel1.CreateConsumer("TestQueue", "TestConsumer", PipeScheduler.ThreadPool, noAck: true);
            consumer1.Received += async (sender, result) =>
            {
                //await channel1.Ack(deliver.DeliveryTag, true);
                var propertiesConsume = ContentHeaderProperties.Default();
                propertiesConsume.AppId = "testapp2";
                await channel2.Publish("TestExchange2", string.Empty, false, false, propertiesConsume, body1);

            };

            var consumer2 = channel2.CreateConsumer("TestQueue2", "TestConsumer2", noAck: true);
            consumer2.Received += async (sender, result) =>
            {
                //await channel2.Ack(deliver.DeliveryTag, true);
                var propertiesConsume = ContentHeaderProperties.Default();
                propertiesConsume.AppId = "testapp1";
                await channel1.Publish("TestExchange", string.Empty, false, false, propertiesConsume, body1);
            };
            await consumer1.ConsumerStartAsync();
            await consumer2.ConsumerStartAsync();
            var firtsTask = Task.Run(async () =>
            {
                var properties = ContentHeaderProperties.Default();
                properties.AppId = "testapp1";
                while (!channel1.IsClosed)
                {
                    await channel1.Publish("TestExchange", string.Empty, false, false, properties, body1);
                }
            });
            var secondTask = Task.Run(async () =>
            {
                var properties = ContentHeaderProperties.Default();
                properties.AppId = "testapp2";
                while (!channel2.IsClosed)
                {
                    await channel2.Publish("TestExchange2", string.Empty, false, false, properties, body1);
                }
            });

            await connection.WaitEndReading();
        }
        private static async Task RunDefault()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });

            var queueOk = await channel.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");

            var properties = ContentHeaderProperties.Default();
            properties.AppId = "testapp";
            for (var i = 0; i < 40000; i++)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                await channel.Publish("TestExchange", string.Empty, false, false, properties, new byte[32]);
            }

            var consumer = channel.CreateConsumer("TestQueue", "TestConsumer", PipeScheduler.ThreadPool, noAck: true);
            consumer.Received += (sender, result) =>
            {
                // await channel.Ack(deliver.DeliveryTag);
            };
            await consumer.ConsumerStartAsync();
            await connection.WaitEndReading();
        }
        private static async Task StartPublisher()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            var queueOk1 = await channel.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");

            var properties = ContentHeaderProperties.Default();
            properties.AppId = "testapp";
            var body = new byte[Size];
            while (!channel.IsClosed)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
            }
            await connection.WaitEndReading();

        }
        private static async Task StartConsumer()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();

            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            var queueOk1 = await channel.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");

            var consumer = channel.CreateConsumer("TestQueue", "TestConsumer", noAck: true);
            consumer.Received += (sender, result) =>
            {
                //await channel.Ack(deliver.DeliveryTag, false);
            };
            await consumer.ConsumerStartAsync();
            //await channel.CloseChannelAsync("kek");
            await connection.WaitEndReading();
        }

        private static async Task RunNothing()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            //Action waiter = async () => {
            //    var info = await channel.WaitClosing();
            //    Console.WriteLine($"Channel closed with: ReplyCode={info.ReplyCode} FailedClassId={info.FailedClassId} FailedMethodId={info.FailedMethodId} ReplyText={info.ReplyText}");
            //};
            await channel.CloseAsync("kek");
            //waiter();
            //await connection.CloseConnection();
            await connection.WaitEndReading();
        }
    }

}
