using System;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.Buffers;
using AMQP.Client.RabbitMQ;
using AMQP.Client.Abstractions;

namespace Test
{

    class Program
    {
        
        static async Task Main(string[] args)
        {
            var connection = await new RabbitMQConnectionBuilder(IPEndPoint.Parse("192.168.66.150:5672")).BuildAsync();           
        }
    }

}
