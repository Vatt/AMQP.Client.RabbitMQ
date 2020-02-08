using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.MethodReaders;
using Microsoft.AspNetCore.Connections;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
namespace Test
{

    class Program
    {        
        static async Task Main(string[] args)
        {
            //var size = Unsafe.SizeOf<ConnectionContext>();
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
        }
    }

}
