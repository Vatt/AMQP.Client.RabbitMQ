using AMQP.Client.RabbitMQ;
using Bedrock.Framework;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;
/*
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var client = new ClientBuilder(serviceProvider)
                         .UseSockets()
                         .Build();
            var connection = await client.ConnectAsync(_endpoint);
            var writeResult = await connection.Transport.Output.WriteAsync(RabbitMQConnection.ProtocolMsg);
             var readResult = await connection.Transport.Input.ReadAsync();
             RabbitMQServerInfo Info = FrameDecoder.DecodeStartMethodFrame(readResult.Buffer);
             if (Info.Major != 0 && Info.Minor != 9)
             {
                DecoderThrowHelper.ThrowFrameDecoderAMQPVersionMissmatch();
             }
             Console.WriteLine($"Connected to {connection.RemoteEndPoint}");
             Console.WriteLine($"ServerInfo:");
             Console.WriteLine($"Major:{Info.Major}; Minor:{Info.Minor}");
             foreach (var pair in Info.Properties)
             {
                 Console.WriteLine($"{pair.Key}:{pair.Value}");
                 if (pair.Value is Dictionary<string, object>)
                 {
                     foreach (var pair1 in pair.Value as Dictionary<string, object>)
                     {
                         Console.WriteLine($"{pair1.Key}:{pair1.Value}");
                     }
                 }
             }
             Console.WriteLine($"Mechanisms:{Info.Mechanisms}");
             Console.WriteLine($"Locales:{Info.Locales}");
             
            return RabbitMQConnection.From(connection,Info); 
*/
namespace Test
{

    class Program
    {
        
        static async Task Main(string[] args)
        {
            RabbitMQConnection connection = new RabbitMQConnection();
            await connection.StartAsync(IPEndPoint.Parse("192.168.66.150:5672"));
        }
    }

}
