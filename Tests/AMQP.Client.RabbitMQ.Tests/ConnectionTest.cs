using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class ConnectionTest : TestBase
    {
        public ConnectionTest() : base()
        {
            
        }
        [Fact]
        public async Task ConnectAndClose()
        {
            var factory = RabbitMQConnectionFactory.Create(new DnsEndPoint(Host, 5672), builder =>
            {
                var loggerFactory = LoggerFactory.Create(loggerBuilder =>
                {
                    loggerBuilder.AddConsole();
                    loggerBuilder.SetMinimumLevel(LogLevel.Debug);
                });
                builder.AddLogger(loggerFactory.CreateLogger(string.Empty));
            });
            try
            {
                var connection = factory.CreateConnection();
                await connection.StartAsync();

                await connection.CloseAsync();
            }
            catch (Exception e)
            {
                Debug.Assert(false);
            }

        }
    }
}