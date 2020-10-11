using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class ChannelTest : TestBase
    {
        private TaskCompletionSource _closedCloseTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        public ChannelTest() : base()
        {
            
        }

        [Fact]
        public async Task OpenAndClose()
        {
            try
            {
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
                channel.ChanelClosed += (sender, args) =>
                {
                    _closedCloseTcs.SetResult();
                };
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Seconds));

                using (var timeoutRegistration = cts.Token.Register(() => _closedCloseTcs.SetCanceled()))
                {
                    await channel.CloseAsync();
                    await connection.CloseAsync();
                    await _closedCloseTcs.Task;
                }
            }
            catch (Exception e)
            {
                Assert.True(false);
            }
            Assert.True(_closedCloseTcs.Task.IsCompleted && !_closedCloseTcs.Task.IsCanceled);
        }

        [Fact]
        public async Task CloseChannelWithRabbitMQException()
        {
            try
            {
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
                channel.ChanelClosed += (sender, args) =>
                {
                    _closedCloseTcs.SetResult();
                };
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Seconds));
                var failTrigger = new RabbitMQConsumer(channel, ConsumeConf.CreateNoWait(channel.ChannelId, "FAILFAILFAIL", "FailConsumer", true));
                using (var timeoutRegistration = cts.Token.Register(() => _closedCloseTcs.SetCanceled()))
                {
                    await channel.ConsumerStartAsync(failTrigger);
                    await connection.CloseAsync();
                    await _closedCloseTcs.Task;
                }
            }
            catch (Exception e)
            {
                Assert.True(false);
            }
            Assert.True(_closedCloseTcs.Task.IsCompleted && !_closedCloseTcs.Task.IsCanceled);
        }
        
    }
}