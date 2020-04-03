using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Channel;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using Xunit;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class TestFloodPublishing
    {
        [Fact]
        public async Task TestMultithreadFloodPublishing()
        {
            string message = "test message";
            int threadCount = 16;
            int publishCount = 200;
            var receivedCount = 0;
            byte[] sendBody = Encoding.UTF8.GetBytes(message);

            var builder = new RabbitMQConnectionFactoryBuilder(new IPEndPoint(IPAddress.Loopback, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel1 = await connection.CreateChannel();

            var queueOk1 = await channel1.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });

            var consumer1 = channel1.CreateConsumer("TestQueue", "TestConsumer", PipeScheduler.ThreadPool, noAck: true);
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

            await consumer1.ConsumerStartAsync();
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
            await consumer1.CancelAsync();



            Assert.Equal(threadCount * publishCount, receivedCount);



            async Task StartFloodAsync(RabbitMQChannel channel, string queue, byte[] body, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    var propertiesConsume = ContentHeaderProperties.Default();
                    await channel1.Publish(string.Empty, queue, false, false, propertiesConsume, body);
                }
            }
        }
    }
}
