using AMQP.Client.RabbitMQ;
using System.Net;
using System.Threading.Tasks;

namespace ConsumerTest
{
    class Program
    {
        private const string Host = "centos0.mshome.net";
        static async Task Main(string[] args)
        {
            var builder = new RabbitMQConnectionFactoryBuilder(new DnsEndPoint(Host, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            var consumer = channel.CreateConsumer("TestQueue", "TestConsumer", noAck: true);

            consumer.Received += (gavno, result) =>
            {

                //await channel.Ack(deliver.DeliveryTag);
            };
            await consumer.ConsumerStartAsync();
            await connection.WaitEndReading();

        }
    }
}
