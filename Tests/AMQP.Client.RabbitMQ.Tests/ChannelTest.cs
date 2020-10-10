using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class ChannelTest : TestBase
    {
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
                try
                {
                    var connection = factory.CreateConnection();
                    await connection.StartAsync();
                    var channel = await connection.OpenChannel();
                    await channel.CloseAsync();
                    await connection.CloseAsync();
                }
                catch (Exception e)
                {
                    Debug.Assert(false);
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false);
            }
        }
        
    }
}