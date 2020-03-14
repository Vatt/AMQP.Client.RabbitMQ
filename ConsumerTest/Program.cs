﻿using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Consumer;
using System;
using System.IO.Pipelines;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ConsumerTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            RabbitMQConnectionFactoryBuilder builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint("centos0.mshome.net", 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Heartbeat(60 * 10)
                                 .ProductName("AMQP.Client.RabbitMQ")
                                 .ProductVersion("0.0.1")
                                 .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                 .ClientInformation("TEST TEST TEST")
                                 .ClientCopyright("©")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            var consumer = channel.CreateConsumer("TestQueue", "TestConsumer", PipeScheduler.ThreadPool, noAck: true);
            consumer.Received += (gavno, result) =>
            {
                
                //await channel.Ack(deliver.DeliveryTag);
            };
            await consumer.ConsumerStartAsync();
            await connection.WaitEndReading();
            
        }
    }
}
