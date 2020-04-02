using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Net;
using System.Threading.Tasks;
namespace Test
{
    class Program
    {
        private const string Host = "centos0.mshome.net";

        //private static string Host = 
        static async Task Main(string[] args)
        {
            //using Microsoft.Extensions.ObjectPool;
            //private static ObjectPool<FrameContentReader> _readerPool = ObjectPool.Create<FrameContentReader>();

            //Utf8JsonWriter
            //Utf8JsonReader
            //JsonSerializer 

            //await RunNothing();
            //await RunDefault();
            //await ChannelTest();

            await RunDefault();
            //await Task.WhenAll(StartConsumer(), StartPublisher());
        }
        public static async Task RunDefault()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.Heartbeat(60).Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            RabbitMQChannel channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(Exchange.Create("TestExchange", ExchangeType.Direct));
            await channel.ExchangeDeclareAsync(Exchange.CreateNoWait("TestExchange2", ExchangeType.Direct));
            await channel.QueueDeclareAsync(Queue.Create("TestQueue"));
            await channel.QueueDeclareNoWaitAsync(Queue.Create("TestQueue2"));

            await channel.QueueDeleteAsync(QueueDelete.Create("TestQueue"));
            await channel.QueueDeleteNoWaitAsync(QueueDelete.Create("TestQueue2"));
            await channel.ExchangeDeleteAsync(ExchangeDelete.Create("TestExchange"));
            await channel.ExchangeDeleteAsync(ExchangeDelete.CreateNoWait("TestExchange2"));

            await connection.CloseAsync();
            await Task.Delay(TimeSpan.FromHours(2));
        }
        public static async Task ChannelTest()
        {
            /*
            var builder = new RabbitMQConnectionFactoryBuilder1(new DnsEndPoint(Host, 5672));
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

            var body1 = new byte[32];


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

            await Task.Delay(TimeSpan.FromHours(1));
            */
        }
        private static async Task StartPublisher()
        {
            /*
            var builder = new RabbitMQConnectionFactoryBuilder1(new DnsEndPoint(Host, 5672));
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
            //var body = new byte[16 * 1024 * 1024 + 1];
            var body = new byte[32];
            //var body = new byte[16*1024];
            //var body = new byte[1024];
            while (!channel.IsClosed)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
            }
            await Task.Delay(TimeSpan.FromHours(1));
            */
        }
        private static async Task StartConsumer()
        {
            /*
            var builder = new RabbitMQConnectionFactoryBuilder1(new DnsEndPoint(Host, 5672));
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
            await Task.Delay(TimeSpan.FromHours(1));
            */
        }

        private static async Task RunNothing()
        {
            /*
            var builder = new RabbitMQConnectionFactoryBuilder1(new DnsEndPoint(Host, 5672));
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
            await Task.Delay(TimeSpan.FromHours(1));
            */
        }
    }

}
