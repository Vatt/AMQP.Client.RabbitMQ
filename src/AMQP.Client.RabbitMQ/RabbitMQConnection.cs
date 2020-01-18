using AMQP.Client.Abstractions;
using Microsoft.AspNetCore.Connections;
using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnection : AMQPConnection
    {
        public static readonly byte[] ProtocolMsg = new byte[8] { 65, 77, 81, 80, 0, 0, 9, 1 };
        public ServerInfo Info { get; private set; }
        public RabbitMQConnection(ConnectionContext context, AMQPApiVersion apiVersion, ServerInfo info)
            :base(context, apiVersion)
        {
            Info = info;
        }

        public override async void Start()
        {
            /*
            var writeResult = await Transport.Output.WriteAsync(ProtocolMsg);
            var readResult = await Transport.Input.ReadAsync();
            Info = FrameDecoder.DecodeStartMethodFrame(readResult.Buffer.FirstSpan);
            if (Info.Major != ApiVersion.Major && Info.Minor != ApiVersion.Minor)
            {
                throw new Exception("FrameDecoder: AMQP version missmatch");
            }
            //Transport.Input.AdvanceTo(result.Buffer.Start);
            Console.WriteLine($"Connected to {RemoteEndPoint}");
            Console.WriteLine($"ServerInfo:");
            Console.WriteLine($"Major:{Info.Major}; Minor:{Info.Minor}");
            foreach (var pair in Info.Properties)
            {
                Console.WriteLine($"{pair.Key}:{pair.Value}");
            }
            Console.WriteLine($"Mechanisms:{Info.Mechanisms}");
            Console.WriteLine($"Locales:{Info.Locales}");
            */
        }
    }
}
