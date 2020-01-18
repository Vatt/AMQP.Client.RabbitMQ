using AMQP.Client.Abstractions;
using System.Net;
using Bedrock.Framework;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnectionBuilder : AMQPConnectionBuilder
    {
        public RabbitMQConnectionBuilder(IPEndPoint endpoint)
            :base(endpoint)
        {

        }
        public override async Task<AMQPConnection> BuildAsync()
        {
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var client = new ClientBuilder(serviceProvider)
                         .UseSockets()
                         .Build();
            var connection = await client.ConnectAsync(_endpoint);
            var writeResult = await connection.Transport.Output.WriteAsync(RabbitMQConnection.ProtocolMsg);
            var readResult = await connection.Transport.Input.ReadAsync();
            ServerInfo Info = FrameDecoder.DecodeStartMethodFrame(readResult.Buffer.FirstSpan);
            if (Info.Major != 0 && Info.Minor != 9)
            {
                throw new Exception("FrameDecoder: AMQP version missmatch");
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

            return new RabbitMQConnection(connection, new AMQPApiVersion(0,9,1),Info);

        }
    }
}
