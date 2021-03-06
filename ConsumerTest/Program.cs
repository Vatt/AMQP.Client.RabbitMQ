﻿using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ConsumerTest
{
    internal class Program
    {
        private const string Host = "centos0.mshome.net";

        private static async Task Main(string[] args)
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.Build();
            var connection = factory.CreateConnection();

            await connection.StartAsync();

            var channel = await connection.OpenChannel();

            await channel.ExchangeDeclareAsync(ExchangeDeclare.Create("TestExchange", ExchangeType.Direct));
            await channel.QueueDeclareAsync(QueueDeclare.Create("TestQueue"));
            await channel.QueueBindAsync(QueueBind.Create("TestQueue", "TestExchange"));

            var consumer = new RabbitMQConsumer(channel, ConsumeConf.Create("TestQueue", "TestConsumer", true));
            consumer.Received += /*async*/ (sender, result) =>
            {
                //await channel.Ack(AckInfo.Create(result.DeliveryTag));
            };
            await channel.ConsumerStartAsync(consumer);
            await Task.Delay(TimeSpan.FromHours(1));
        }
    }
}