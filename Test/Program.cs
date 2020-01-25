using AMQP.Client.RabbitMQ;
using System.Net;
using System.Threading.Tasks;
namespace Test
{

    class Program
    {
        
        static async Task Main(string[] args)
        {
             RabbitMQConnection connection = new RabbitMQConnection();
             await connection.StartAsync(IPEndPoint.Parse("172.17.72.156:5672"));

            
        }
    }

}
