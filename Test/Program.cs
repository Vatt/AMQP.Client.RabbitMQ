using AMQP.Client.RabbitMQ;
using System.Net;
using System.Threading.Tasks;
namespace Test
{

    class Program
    {
        
        static async Task Main(string[] args)
        {
            RabbitMQConnectionBuilder builder = new RabbitMQConnectionBuilder(IPEndPoint.Parse("172.17.72.151:5672"));
            var connection = builder.ConnectionInfo("gamover", "gam2106", "/")
                                    .Heartbeat(5)
                                    .ProductName("AMQP.Client.RabbitMQ")
                                    .ProductVersion("0.0.1")
                                    .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                    .ClientInformation("TEST TEST TEST")
                                    .ClientCopyright("©")
                                    .Build();
            await connection.StartAsync();
        }
    }

}
