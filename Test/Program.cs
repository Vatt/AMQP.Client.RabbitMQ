using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading.Tasks;

namespace Test
{
    //Закрытие канала от сервера проверить можно кривым указанием очереди в консумере
    //на 16 мегах чтото непонятное творится

    internal class Program
    {
        private static string Host = "centos0.mshome.net";
        //private static int Size = 1024 * 1024; //32;
        private static int Size = 32;

        //private static string Host = 
        private static async Task Main(string host, int size)
        {
            //using Microsoft.Extensions.ObjectPool;
            //private static ObjectPool<FrameContentReader> _readerPool = ObjectPool.Create<FrameContentReader>();

            //Utf8JsonWriter
            //Utf8JsonReader
            //JsonSerializer 

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
            await ChannelTest();
            //await Task.WhenAll(StartConsumer(), StartPublisher());

        }

        private static async Task StartPublisher()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.AddLogger(loggerFactory.CreateLogger(string.Empty))
                                 .ConnectionTimeout(TimeSpan.FromSeconds(30))
                                 .ConnectionAttempts(100)
                                 .Build();
            var connection = factory.CreateConnection();

            await connection.StartAsync();

            var channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create("TestExchange", ExchangeType.Direct));
            await channel.QueueDeclareAsync(QueueDeclare.Create("TestQueue"));
            await channel.QueueBindAsync(QueueBind.Create("TestQueue", "TestExchange"));

            var properties = new ContentHeaderProperties();
            properties.AppId = "testapp";
            var body = new byte[Size];
            int i = 0;
            while (true/*!channel.IsClosed*/)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                var result = await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
                //if (!result)
                //{
                //    break;
                //}
            }

            //await Task.Delay(TimeSpan.FromHours(1));
        }

        private static async Task StartConsumer()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.AddLogger(loggerFactory.CreateLogger(string.Empty)).Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();

            var channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create("TestExchange", ExchangeType.Direct));
            await channel.QueueDeclareAsync(QueueDeclare.Create("TestQueue"));
            await channel.QueueBindAsync(QueueBind.Create("TestQueue", "TestExchange"));

            var consumer = new RabbitMQConsumer(channel, ConsumeConf.Create("TestQueue", "TestConsumer", true), PipeScheduler.ThreadPool);
            consumer.Received += /*async*/ (sender, result) =>
            {
                //await channel.Ack(AckInfo.Create(result.DeliveryTag));
            };
            await channel.ConsumerStartAsync(consumer);
            await Task.Delay(TimeSpan.FromHours(1));
        }

        public static async Task ChannelTest()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.AddLogger(loggerFactory.CreateLogger(string.Empty))
                                 .ConnectionTimeout(TimeSpan.FromSeconds(30))
                                 .ConnectionAttempts(10)
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel1 = await connection.OpenChannel();
            var channel2 = await connection.OpenChannel();
            await channel1.ExchangeDeclareAsync(ExchangeDeclare.Create("TestExchange", ExchangeType.Direct));
            await channel1.ExchangeDeclareAsync(ExchangeDeclare.CreateNoWait("TestExchange2", ExchangeType.Direct));
            var declareOk = await channel1.QueueDeclareAsync(QueueDeclare.Create("TestQueue"));
            await channel1.QueueDeclareNoWaitAsync(QueueDeclare.Create("TestQueue2"));
            var purgeOk = await channel1.QueuePurgeAsync(QueuePurge.Create("TestQueue"));
            await channel1.QueuePurgeNoWaitAsync(QueuePurge.Create("TestQueue2"));
            await channel1.QueueBindAsync(QueueBind.Create("TestQueue", "TestExchange"));
            await channel1.QueueBindAsync(QueueBind.CreateNoWait("TestQueue2", "TestExchange2"));

            var body1 = new byte[Size];


            var consumer1 = new RabbitMQConsumer(channel1, ConsumeConf.Create("TestQueue", "TestConsumer", true));
            consumer1.Received += async (sender, result) =>
            {
                //await channel1.Ack(deliver.DeliveryTag, true);
                var propertiesConsume = new ContentHeaderProperties();
                propertiesConsume.AppId = "testapp2";
                var published =  await channel2.Publish("TestExchange2", string.Empty, false, false, propertiesConsume, body1);
            };

            var consumer2 = new RabbitMQConsumer(channel2, ConsumeConf.Create("TestQueue2", "TestConsumer2", true));
            consumer2.Received += async (sender, result) =>
            {
                //await channel2.Ack(deliver.DeliveryTag, true);
                var propertiesConsume = new ContentHeaderProperties();
                propertiesConsume.AppId = "testapp1";
                var published =  await channel1.Publish("TestExchange", string.Empty, false, false, propertiesConsume, body1);
            };
            await channel1.ConsumerStartAsync(consumer1);
            await channel2.ConsumerStartAsync(consumer2);

            var firtsTask = Task.Run(async () =>
            {
                var properties = new ContentHeaderProperties();
                properties.AppId = "testapp1";
                while (true /*!channel1.IsClosed*/)
                {
                    var published = await channel1.Publish("TestExchange", string.Empty, false, false, properties, body1);
                }
            });
            var secondTask = Task.Run(async () =>
            {
                var properties = new ContentHeaderProperties();
                properties.AppId = "testapp2";
                while (true /*!channel2.IsClosed*/)
                {
                    var published = await channel2.Publish("TestExchange2", string.Empty, false, false, properties, body1);
                }
            });

            await Task.Delay(TimeSpan.FromHours(1));
        }

        public static async Task RunDefault()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create("TestExchange", ExchangeType.Direct));
            await channel.ExchangeDeclareAsync(ExchangeDeclare.CreateNoWait("TestExchange2", ExchangeType.Direct));
            var declareOk = await channel.QueueDeclareAsync(QueueDeclare.Create("TestQueue"));
            await channel.QueueDeclareNoWaitAsync(QueueDeclare.Create("TestQueue2"));
            var purgeOk = await channel.QueuePurgeAsync(QueuePurge.Create("TestQueue"));
            await channel.QueuePurgeNoWaitAsync(QueuePurge.Create("TestQueue2"));
            await channel.QueueBindAsync(QueueBind.Create("TestQueue", "TestExchange"));
            await channel.QueueBindAsync(QueueBind.CreateNoWait("TestQueue2", "TestExchange2"));

            var consumer = new RabbitMQConsumer(channel, ConsumeConf.Create("TestQueue", "TestConsumer", true));

            consumer.Received += (sender, result) =>
            {
                //await channel.Ack(deliver.DeliveryTag, false);
                //Console.WriteLine(Encoding.UTF8.GetString(result.Body));
            };
            await channel.ConsumerStartAsync(consumer);
            await channel.Publish("TestExchange", string.Empty, false, false, new ContentHeaderProperties(), new byte[16 * 1024 * 1024 + 1]);

            //await channel.QueueUnbindAsync(QueueUnbind.Create("TestQueue", "TestExchange"));
            //await channel.QueueUnbindAsync(QueueUnbind.Create("TestQueue2", "TestExchange2"));
            //var deleteOk = await channel.QueueDeleteAsync(QueueDelete.Create("TestQueue"));
            //await channel.QueueDeleteNoWaitAsync(QueueDelete.Create("TestQueue2"));
            //await channel.ExchangeDeleteAsync(ExchangeDelete.Create("TestExchange"));
            //await channel.ExchangeDeleteAsync(ExchangeDelete.CreateNoWait("TestExchange2"));
            await connection.CloseAsync();

            await Task.Delay(TimeSpan.FromHours(2));
        }
    }
}