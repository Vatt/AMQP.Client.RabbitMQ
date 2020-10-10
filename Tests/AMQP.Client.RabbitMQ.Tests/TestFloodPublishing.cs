using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using Microsoft.Extensions.Logging;
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
        private static readonly int threadCount = Environment.ProcessorCount;
        private static readonly int publishCount = threadCount * 100;
        private static readonly int seconds = 2000;
        private static string Host = "centos0.mshome.net";
        public TestFloodPublishing()
        {
            Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "centos0.mshome.net";
        }

        [Fact]
        public async Task TestMultithreadFloodPublishingNoAck()
        {
            var receivedCount = 0;
            byte[] sendBody = Encoding.UTF8.GetBytes(message);

            var factory = RabbitMQConnectionFactory.Create(new DnsEndPoint(Host, 5672), builder =>
            {
                var loggerFactory = LoggerFactory.Create(loggerBuilder =>
                {
                    loggerBuilder.AddConsole();
                });
                builder.AddLogger(loggerFactory.CreateLogger(string.Empty));
            });
            var connection = factory.CreateConnection();
            await connection.StartAsync();

            var channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create(channel.ChannelId, "TestExchange", ExchangeType.Direct));
            var queueOk = await channel.QueueDeclareAsync(QueueDeclare.Create(channel.ChannelId, "TestQueue", arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } }));
            await channel.QueueBindAsync(QueueBind.Create(channel.ChannelId, "TestQueue", "TestExchange"));

            var consumer = new RabbitMQConsumer(channel, ConsumeConf.Create(channel.ChannelId, "TestQueue", "TestConsumerNoAck", true), PipeScheduler.ThreadPool);
            await channel.ConsumerStartAsync(consumer);

            var tcs = new TaskCompletionSource<bool>();
            consumer.Received += (sender, result) =>
            {
                Assert.Equal(message, Encoding.UTF8.GetString(result.Body));

                var inc = Interlocked.Increment(ref receivedCount);
                if (inc == threadCount * publishCount)
                {
                    tcs.SetResult(true);
                }

            };


            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));

            using (var timeoutRegistration = cts.Token.Register(() => tcs.SetCanceled()))
            {
                var tasks = new List<Task>();
                for (int i = 0; i < threadCount; i++)
                {
                    var task = StartFloodAsync(channel,  "TestExchange", sendBody, publishCount);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
                await tcs.Task;
            }
            //await consumer1.CancelAsync(); //TODOL fix this     

            Assert.Equal(threadCount * publishCount, receivedCount);

            await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "TestQueue", "TestExchange"));
            var deleted = await channel.QueueDeleteAsync(QueueDelete.Create(channel.ChannelId, "TestQueue"));
            await channel.ExchangeDeleteAsync(ExchangeDelete.Create(channel.ChannelId, "TestExchange"));
            await connection.CloseAsync("Finish TestMultithreadFloodPublishingNoAck");
        }

        [Fact]
        public async Task TestMultithreadFloodPublishingWithAck()
        {

            var receivedCount = 0;
            byte[] sendBody = Encoding.UTF8.GetBytes(message);

            var factory = RabbitMQConnectionFactory.Create(new DnsEndPoint(Host, 5672), builder =>
            {
                var loggerFactory = LoggerFactory.Create(loggerBuilder =>
                {
                    loggerBuilder.AddConsole();
                });
                builder.AddLogger(loggerFactory.CreateLogger(string.Empty));
            });
            var connection = factory.CreateConnection();
            await connection.StartAsync();

            var channel = await connection.OpenChannel();



            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create(channel.ChannelId, "TestExchange", ExchangeType.Direct));
            var queueOk = await channel.QueueDeclareAsync(QueueDeclare.Create(channel.ChannelId, "TestQueue", arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } }));
            await channel.QueueBindAsync(QueueBind.Create(channel.ChannelId, "TestQueue", "TestExchange"));

            var consumer = new RabbitMQConsumer(channel, ConsumeConf.Create(channel.ChannelId, "TestQueue", "TestConsumerWithAck"), PipeScheduler.ThreadPool);

            var tcs = new TaskCompletionSource<bool>();
            consumer.Received += async (sender, result) =>
            {

                Assert.Equal(message, Encoding.UTF8.GetString(result.Body));
                var inc = Interlocked.Increment(ref receivedCount);
                if (inc == threadCount * publishCount)
                {
                    tcs.SetResult(true);
                }
                await channel.Ack(AckInfo.Create(channel.ChannelId, result.DeliveryTag));

            };

            await channel.ConsumerStartAsync(consumer);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));

            using (var timeoutRegistration = cts.Token.Register(() => tcs.SetCanceled()))
            {
                var tasks = new List<Task>();
                for (int i = 0; i < threadCount; i++)
                {
                    var task = StartFloodAsync(channel, "TestExchange", sendBody, publishCount);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
                await tcs.Task;

            }
            //await consumer1.CancelAsync(); //TODO: fix this
            Assert.Equal(threadCount * publishCount, receivedCount);

            await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "TestQueue", "TestExchange"));
            var deleted = await channel.QueueDeleteAsync(QueueDelete.Create(channel.ChannelId, "TestQueue"));
            await channel.ExchangeDeleteAsync(ExchangeDelete.Create(channel.ChannelId, "TestExchange"));

            await connection.CloseAsync("Finish TestMultithreadFloodPublishingWithAck");
        }
        async Task StartFloodAsync(RabbitMQChannel channel, string exchange, byte[] body, int count)
        {
            var propertiesConsume = new ContentHeaderProperties();
            for (int i = 0; i < count; i++)
            {
                await channel.Publish(exchange, string.Empty, false, false, propertiesConsume, body);
            }
        }
    }
}
