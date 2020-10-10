using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class QueueAndExchangeTest : TestBase
    {
        public QueueAndExchangeTest() : base()
        {
            
        }
        [Fact]
        public async Task CreateQueueCreateExchangeBindAndDelete()
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
            try
            {  
                await channel.QueueDeclareAsync(QueueDeclare.Create(channel.ChannelId, "xUnitTestQueueForDirect"));
                await channel.QueueDeclareAsync(QueueDeclare.Create(channel.ChannelId, "xUnitTestQueueForFanout"));
                await channel.QueueDeclareAsync(QueueDeclare.Create(channel.ChannelId, "xUnitTestQueueForHeaders"));
                await channel.QueueDeclareAsync(QueueDeclare.Create(channel.ChannelId, "xUnitTestQueueForTopic"));
                
                await channel.ExchangeDeclareAsync(ExchangeDeclare.Create(channel.ChannelId, "xUnitTestExchangeDirect", ExchangeType.Direct));
                await channel.ExchangeDeclareAsync(ExchangeDeclare.Create(channel.ChannelId, "xUnitTestExchangeFanout", ExchangeType.Fanout));
                await channel.ExchangeDeclareAsync(ExchangeDeclare.Create(channel.ChannelId, "xUnitTestExchangeHeaders", ExchangeType.Headers));
                await channel.ExchangeDeclareAsync(ExchangeDeclare.Create(channel.ChannelId, "xUnitTestExchangeTopic", ExchangeType.Topic));

                await channel.QueueBindAsync(QueueBind.Create(channel.ChannelId, "xUnitTestQueueForDirect", "xUnitTestExchangeDirect"));
                await channel.QueueBindAsync(QueueBind.Create(channel.ChannelId, "xUnitTestQueueForFanout", "xUnitTestExchangeFanout"));
                await channel.QueueBindAsync(QueueBind.Create(channel.ChannelId, "xUnitTestQueueForHeaders", "xUnitTestExchangeHeaders"));
                await channel.QueueBindAsync(QueueBind.Create(channel.ChannelId, "xUnitTestQueueForTopic", "xUnitTestExchangeTopic"));

                await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "xUnitTestQueueForDirect", "xUnitTestExchangeDirect"));
                await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "xUnitTestExchangeFanout", "xUnitTestExchangeFanout"));
                await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "xUnitTestExchangeHeaders", "xUnitTestExchangeHeaders"));
                await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "xUnitTestQueueForTopic", "xUnitTestExchangeTopic"));
                
                
                
                await channel.QueueDeleteAsync(QueueDelete.Create(channel.ChannelId, "xUnitTestQueueForDirect"));
                await channel.QueueDeleteAsync(QueueDelete.Create(channel.ChannelId, "xUnitTestQueueForFanout"));
                await channel.QueueDeleteAsync(QueueDelete.Create(channel.ChannelId, "xUnitTestQueueForHeaders"));
                await channel.QueueDeleteAsync(QueueDelete.Create(channel.ChannelId, "xUnitTestQueueForTopic"));

                await channel.ExchangeDeleteAsync(ExchangeDelete.Create(channel.ChannelId, "xUnitTestExchangeDirect"));
                await channel.ExchangeDeleteAsync(ExchangeDelete.Create(channel.ChannelId, "xUnitTestExchangeFanout"));
                await channel.ExchangeDeleteAsync(ExchangeDelete.Create(channel.ChannelId, "xUnitTestExchangeHeaders"));
                await channel.ExchangeDeleteAsync(ExchangeDelete.Create(channel.ChannelId, "xUnitTestExchangeTopic"));
            }
            catch(Exception e)
            {
                Debug.Assert(false);
            }

        }
        [Fact]
        public async Task CreateQueueCreateExchangeBindAndDeleteNoWait()
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
            try
            {  
                await channel.QueueDeclareNoWaitAsync(QueueDeclare.Create(channel.ChannelId, "xUnitTestQueueForDirect"));
                await channel.QueueDeclareNoWaitAsync(QueueDeclare.Create(channel.ChannelId, "xUnitTestQueueForFanout"));
                await channel.QueueDeclareNoWaitAsync(QueueDeclare.Create(channel.ChannelId, "xUnitTestQueueForHeaders"));
                await channel.QueueDeclareNoWaitAsync(QueueDeclare.Create(channel.ChannelId, "xUnitTestQueueForTopic"));
                
                await channel.ExchangeDeclareAsync(ExchangeDeclare.CreateNoWait(channel.ChannelId, "xUnitTestExchangeDirect", ExchangeType.Direct));
                await channel.ExchangeDeclareAsync(ExchangeDeclare.CreateNoWait(channel.ChannelId, "xUnitTestExchangeFanout", ExchangeType.Fanout));
                await channel.ExchangeDeclareAsync(ExchangeDeclare.CreateNoWait(channel.ChannelId, "xUnitTestExchangeHeaders", ExchangeType.Headers));
                await channel.ExchangeDeclareAsync(ExchangeDeclare.CreateNoWait(channel.ChannelId, "xUnitTestExchangeTopic", ExchangeType.Topic));

                await channel.QueueBindAsync(QueueBind.CreateNoWait(channel.ChannelId, "xUnitTestQueueForDirect", "xUnitTestExchangeDirect"));
                await channel.QueueBindAsync(QueueBind.CreateNoWait(channel.ChannelId, "xUnitTestQueueForFanout", "xUnitTestExchangeFanout"));
                await channel.QueueBindAsync(QueueBind.CreateNoWait(channel.ChannelId, "xUnitTestQueueForHeaders", "xUnitTestExchangeHeaders"));
                await channel.QueueBindAsync(QueueBind.CreateNoWait(channel.ChannelId, "xUnitTestQueueForTopic", "xUnitTestExchangeTopic"));

                await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "xUnitTestQueueForDirect", "xUnitTestExchangeDirect"));
                await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "xUnitTestExchangeFanout", "xUnitTestExchangeFanout"));
                await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "xUnitTestExchangeHeaders", "xUnitTestExchangeHeaders"));
                await channel.QueueUnbindAsync(QueueUnbind.Create(channel.ChannelId, "xUnitTestQueueForTopic", "xUnitTestExchangeTopic"));
                
                
                
                await channel.QueueDeleteNoWaitAsync(QueueDelete.Create(channel.ChannelId, "xUnitTestQueueForDirect"));
                await channel.QueueDeleteNoWaitAsync(QueueDelete.Create(channel.ChannelId, "xUnitTestQueueForFanout"));
                await channel.QueueDeleteNoWaitAsync(QueueDelete.Create(channel.ChannelId, "xUnitTestQueueForHeaders"));
                await channel.QueueDeleteNoWaitAsync(QueueDelete.Create(channel.ChannelId, "xUnitTestQueueForTopic"));

                await channel.ExchangeDeleteAsync(ExchangeDelete.CreateNoWait(channel.ChannelId, "xUnitTestExchangeDirect"));
                await channel.ExchangeDeleteAsync(ExchangeDelete.CreateNoWait(channel.ChannelId, "xUnitTestExchangeFanout"));
                await channel.ExchangeDeleteAsync(ExchangeDelete.CreateNoWait(channel.ChannelId, "xUnitTestExchangeHeaders"));
                await channel.ExchangeDeleteAsync(ExchangeDelete.CreateNoWait(channel.ChannelId, "xUnitTestExchangeTopic"));
            }
            catch(Exception e)
            {
                Debug.Assert(false);
            }

        }

        

    }
}