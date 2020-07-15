using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class TestFloodPublishing
    {
        private static readonly string message = "test message";
        private static readonly int threadCount = 16;
        private static readonly int publishCount = 200;
        private static readonly string Host = "centos0.mshome.net";

        private static readonly ExchangeDeclare _exchangeDeclare = ExchangeDeclare.Create("TestExchange", ExchangeType.Direct);
        private static readonly QueueDeclare _queueDeclare = QueueDeclare.Create("TestQueue", arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
        private static readonly QueueBind _queueBind = QueueBind.Create("TestQueue", "TestExchange");
        private static readonly QueueUnbind _queueUnbind = QueueUnbind.Create("TestQueue", "TestExchange");
        private static readonly QueueDelete _queueDelete = QueueDelete.Create("TestQueue");
        private static readonly ExchangeDelete _exchangeDelete = ExchangeDelete.Create("TestExchange");
        private static readonly ConsumeConf _consumerConfNoAck = ConsumeConf.Create("TestQueue", "TestConsumerNoAck", true);
        private static readonly ConsumeConf _consumerConfWithAck = ConsumeConf.Create("TestQueue", "TestConsumerWithAck");

        [Fact]
        public async Task TestMultithreadFloodPublishingNoAck()
        {

            var receivedCount = 0;
            byte[] sendBody = Encoding.UTF8.GetBytes(message);

            //var builder = new RabbitMQConnectionFactoryBuilder(new IPEndPoint(IPAddress.Loopback, 5672));
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();

            var channel = await connection.OpenChannel();

            await channel.QueueUnbindAsync(_queueUnbind);
            var deleted = await channel.QueueDeleteAsync(_queueDelete);
            await channel.ExchangeDeleteAsync(_exchangeDelete);

            await channel.ExchangeDeclareAsync(_exchangeDeclare);
            var queueOk = await channel.QueueDeclareAsync(_queueDeclare);
            await channel.QueueBindAsync(_queueBind);

            var consumer = new RabbitMQConsumer(channel, _consumerConfNoAck, PipeScheduler.ThreadPool);
            await channel.ConsumerStartAsync(consumer);

            var tcs = new TaskCompletionSource<bool>();
            consumer.Received += async (sender, result) =>
            {
                Assert.Equal(message, Encoding.UTF8.GetString(result.Body));

                var inc = Interlocked.Increment(ref receivedCount);
                if (inc == threadCount * publishCount)
                {
                    tcs.SetResult(true);
                }

            };


            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            using (var timeoutRegistration = cts.Token.Register(() => tcs.SetCanceled()))
            {
                var tasks = new List<Task>();
                for (int i = 0; i < threadCount; i++)
                {
                    var task = StartFloodAsync(channel, "TestQueue", sendBody, publishCount);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
                await tcs.Task;
            }
            //await consumer1.CancelAsync(); //TODOL fix this     

            Assert.Equal(threadCount * publishCount, receivedCount);

            await connection.CloseAsync("Finish TestMultithreadFloodPublishingNoAck");
        }
        [Fact]
        public async Task TestMultithreadFloodPublishingWithAck()
        {

            var receivedCount = 0;
            byte[] sendBody = Encoding.UTF8.GetBytes(message);

            //var builder = new RabbitMQConnectionFactoryBuilder(new IPEndPoint(IPAddress.Loopback, 5672));
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();

            var channel = await connection.OpenChannel();


            await channel.QueueUnbindAsync(_queueUnbind);
            var deleted = await channel.QueueDeleteAsync(_queueDelete);
            await channel.ExchangeDeleteAsync(_exchangeDelete);

            await channel.ExchangeDeclareAsync(_exchangeDeclare);
            var queueOk = await channel.QueueDeclareAsync(_queueDeclare);
            await channel.QueueBindAsync(_queueBind);

            var consumer = new RabbitMQConsumer(channel, _consumerConfWithAck, PipeScheduler.ThreadPool);

            var tcs = new TaskCompletionSource<bool>();
            consumer.Received += async (sender, result) =>
            {
                Assert.Equal(message, Encoding.UTF8.GetString(result.Body));
                var inc = Interlocked.Increment(ref receivedCount);
                if (inc == threadCount * publishCount)
                {
                    await channel.Ack(AckInfo.Create(result.DeliveryTag));
                    tcs.SetResult(true);
                    return;
                }
                await channel.Ack(AckInfo.Create(result.DeliveryTag));
            };

            await channel.ConsumerStartAsync(consumer);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));



            using (var timeoutRegistration = cts.Token.Register(() => tcs.SetCanceled()))
            {
                var tasks = new List<Task>();
                for (int i = 0; i < threadCount; i++)
                {
                    var task = StartFloodAsync(channel, "TestQueue", sendBody, publishCount);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
                await tcs.Task;

            }
            //await consumer1.CancelAsync(); //TODOL fix this
            Assert.Equal(threadCount * publishCount, receivedCount);

            await connection.CloseAsync("Finish TestMultithreadFloodPublishingWithAck");
        }
        async Task StartFloodAsync(RabbitMQChannel channel, string queue, byte[] body, int count)
        {
            var propertiesConsume = new ContentHeaderProperties();
            for (int i = 0; i < count; i++)
            {
                await channel.Publish(string.Empty, queue, false, false, propertiesConsume, body);
            }
        }
    }
}
