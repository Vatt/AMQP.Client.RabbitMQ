using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using Xunit;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class TestFloodPublishing
    {
        private static string Host = "centos0.mshome.net";
        [Fact]
        public async Task TestMultithreadFloodPublishing()
        {
            string message = "test message";
            int threadCount = 16;
            int publishCount = 200;
            var receivedCount = 0;
            byte[] sendBody = Encoding.UTF8.GetBytes(message);

            //var builder = new RabbitMQConnectionFactoryBuilder(new IPEndPoint(IPAddress.Loopback, 5672));
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel1 = await connection.OpenChannel();

            var queueOk1 = await channel1.QueueDeclareAsync(QueueDeclare.Create("TestQueue",arguments:new Dictionary<string, object> { { "TEST_ARGUMENT", true } }));

            var consumer1 = new RabbitMQConsumer(channel1, ConsumeConf.Create("TestQueue", "TestConsumer", noAck: true), PipeScheduler.ThreadPool);

            var tcs = new TaskCompletionSource<bool>();
            consumer1.Received += async (sender, result) =>
            {
                Assert.Equal(message, Encoding.UTF8.GetString(result.Body));

                var inc = Interlocked.Increment(ref receivedCount);
                if (inc == threadCount * publishCount)
                {
                    tcs.SetResult(true);
                }

            };

            await channel1.ConsumerStartAsync(consumer1);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));



            using (var timeoutRegistration = cts.Token.Register(() => tcs.SetCanceled()))
            {
                var tasks = new List<Task>();
                for (int i = 0; i < threadCount; i++)
                {
                    var task = StartFloodAsync(channel1, "TestQueue", sendBody, publishCount);
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
                await tcs.Task;
            }
            //await consumer1.CancelAsync(); //TODOL fix this


            Assert.Equal(threadCount * publishCount, receivedCount);



            async Task StartFloodAsync(RabbitMQChannel channel, string queue, byte[] body, int count)
            {
                var propertiesConsume = new ContentHeaderProperties();
                for (int i = 0; i < count; i++)
                {                    
                    await channel1.Publish(string.Empty, queue, false, false, propertiesConsume, body);
                }
            }
        }
    }
}
