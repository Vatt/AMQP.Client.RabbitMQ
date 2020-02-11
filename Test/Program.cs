using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Exchange;
using System.Net;
using System.Threading.Tasks;
namespace Test
{

    class Program
    {        
        static async Task Main(string[] args)
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(new IPEndPoint(address, 5672));
            var connection = builder.ConnectionInfo("gamover", "gam2106", "/")
                                    .Heartbeat(60)
                                    .ProductName("AMQP.Client.RabbitMQ")
                                    .ProductVersion("0.0.1")
                                    .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                    .ClientInformation("TEST TEST TEST")
                                    .ClientCopyright("©")
                                    .Build();

            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            await channel.Exchange()
                         .AsCreate("TextExchange", ExchangeType.Direct)
                         .AutoDelete()
                         .Durable()
                         .WithArgument("TEST_ARGUMENT", true)
                         .DeclareAsync();

            await channel.TryCloseChannelAsync("Channel closing test");
            await connection.WaitEndReading();//for testing
        }

    }

}
