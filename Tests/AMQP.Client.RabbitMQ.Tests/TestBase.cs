using System;

namespace AMQP.Client.RabbitMQ.Tests
{
    public class TestBase
    {
        protected string Message => "test message";
        protected int ThreadCount => Environment.ProcessorCount;
        protected int PublishCount => ThreadCount * 100;
        protected int Seconds => 5;
        protected string Host { get; }

        protected TestBase()
        {
            Host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "centos0.mshome.net";
        }
    }
}