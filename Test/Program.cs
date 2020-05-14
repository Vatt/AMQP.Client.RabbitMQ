using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Test
{
    //Закрытие канала от сервера проверить можно кривым указанием очереди в консумере
    //на 16 мегах чтото непонятное творится
    internal class Program
    {
        private const string Host = "centos0.mshome.net";

        //private static string Host = 
        private static async Task Main(string[] args)
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
            await Task.WhenAll(StartConsumer(),
                StartPublisher());
        }


        private static async Task StartPublisher()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.Build();
            var connection = factory.CreateConnection();

            await connection.StartAsync();

            var channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create("TestExchange", ExchangeType.Direct));
            await channel.QueueDeclareAsync(QueueDeclare.Create("TestQueue"));
            await channel.QueueBindAsync(QueueBind.Create("TestQueue", "TestExchange"));

            var properties = new ContentHeaderProperties();
            properties.AppId = "testapp";
            //var body = new byte[16 * 1024 * 1024 + 1];
            //var body = new byte[16 * 1024 * 1024];
            //var body = new byte[32];
            //var body = new byte[16*1024];
            //var body = new byte[512*1024];
            //var body = new byte[512 * 1024];
            var body = new byte[1 * 1024 * 1024];
            //var body = new byte[1024];

            while (true /*!channel.IsClosed*/)
            {
                properties.CorrelationId = Guid.NewGuid().ToString();
                await channel.Publish("TestExchange", string.Empty, false, false, properties, body);
            }

            //await Task.Delay(TimeSpan.FromHours(1));
        }

        private static async Task StartConsumer()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.Build();
            var connection = factory.CreateConnection();

            await connection.StartAsync();

            var channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create("TestExchange", ExchangeType.Direct));
            await channel.QueueDeclareAsync(QueueDeclare.Create("TestQueue"));
            await channel.QueueBindAsync(QueueBind.Create("TestQueue", "TestExchange"));

            var consumer = new RabbitMQConsumer(channel, ConsumeConf.Create("TestQueue", "TestConsumer", true));
            consumer.Received += /*async*/ (sender, result) =>
            {
                //await channel.Ack(AckInfo.Create(result.DeliveryTag));
            };
            await channel.ConsumerStartAsync(consumer);
            await Task.Delay(TimeSpan.FromHours(1));
        }

        public static async Task ChannelTest()
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.Build();
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

            var body1 = new byte[1024];


            //var publisher1 = channel1.CreatePublisher();
            //var publisher2 = channel2.CreatePublisher();


            var consumer1 = new RabbitMQConsumer(channel1, ConsumeConf.Create("TestQueue", "TestConsumer", true));
            consumer1.Received += async (sender, result) =>
            {
                //await channel1.Ack(deliver.DeliveryTag, true);
                var propertiesConsume = new ContentHeaderProperties();
                propertiesConsume.AppId = "testapp2";
                await channel2.Publish("TestExchange2", string.Empty, false, false, propertiesConsume, body1);
            };

            var consumer2 = new RabbitMQConsumer(channel2, ConsumeConf.Create("TestQueue2", "TestConsumer2", true));
            consumer2.Received += async (sender, result) =>
            {
                //await channel2.Ack(deliver.DeliveryTag, true);
                var propertiesConsume = new ContentHeaderProperties();
                propertiesConsume.AppId = "testapp1";
                await channel1.Publish("TestExchange", string.Empty, false, false, propertiesConsume, body1);
            };
            await channel1.ConsumerStartAsync(consumer1);
            await channel2.ConsumerStartAsync(consumer2);

            var firtsTask = Task.Run(async () =>
            {
                var properties = new ContentHeaderProperties();
                properties.AppId = "testapp1";
                while (true /*!channel1.IsClosed*/
                ) await channel1.Publish("TestExchange", string.Empty, false, false, properties, body1);
            });
            var secondTask = Task.Run(async () =>
            {
                var properties = new ContentHeaderProperties();
                properties.AppId = "testapp2";
                while (true /*!channel2.IsClosed*/
                ) await channel2.Publish("TestExchange2", string.Empty, false, false, properties, body1);
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
            await channel.Publish("TestExchange", string.Empty, false, false, new ContentHeaderProperties(),
                new byte[16 * 1024 * 1024 + 1]);
            //await channel.QueueUnbindAsync(QueueUnbind.Create("TestQueue", "TestExchange"));
            //await channel.QueueUnbindAsync(QueueUnbind.Create("TestQueue2", "TestExchange2"));
            //var deleteOk = await channel.QueueDeleteAsync(QueueDelete.Create("TestQueue"));
            //await channel.QueueDeleteNoWaitAsync(QueueDelete.Create("TestQueue2"));
            //await channel.ExchangeDeleteAsync(ExchangeDelete.Create("TestExchange"));
            //await channel.ExchangeDeleteAsync(ExchangeDelete.CreateNoWait("TestExchange2"));
            //await connection.CloseAsync();

            await Task.Delay(TimeSpan.FromHours(2));
        }
    }
}