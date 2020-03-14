using AMQP.Client.RabbitMQ;
using AMQP.Client.RabbitMQ.Handlers;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace JSONTest
{
    class JSONObject
    {
        public int IntValue { get; set; }
      //  public float FloatFalue { get; set; }
        public string StringValue { get; set; }
        public DateTimeOffset Date { get; set; }
        public Dictionary<string, Dictionary<string, int>> TableValue { get; set; }

        public static JSONObject MakeJSONObject()
        {
            JSONObject json = new JSONObject();
            json.IntValue = 65536;
         //   json.FloatFalue = 65536.1f;
            json.StringValue = "JSONTEST";
            json.Date = DateTimeOffset.Now;
            json.TableValue = new Dictionary<string, Dictionary<string, int>>();
            for (int first = 1; first < 256; first++)
            {
                var inner = new Dictionary<string, int>();
                for (int second = 0; second < 256; second++)
                {
                    inner.Add((first * second).ToString(), second);
                }
                json.TableValue.Add(first.ToString(), inner);
            }
            return json;
        }
    }
    class Program
    {
        static JSONObject Json;
        static async Task Main(string[] args)
        {
            Json = JSONObject.MakeJSONObject();

            Task.WaitAny(RunJSONConsumer().AsTask(),
                        RunJSONPublisher().AsTask());
        }
        public static async ValueTask RunJSONPublisher()
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionFactoryBuilder builder = new RabbitMQConnectionFactoryBuilder(new IPEndPoint(address, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Heartbeat(60)
                                 .ProductName("AMQP.Client.RabbitMQ")
                                 .ProductVersion("0.0.1")
                                 .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                 .ClientInformation("TEST TEST TEST")
                                 .ClientCopyright("©")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");

            var properties = ContentHeaderProperties.Default();
            properties.AppId = "testapp";
            var body = JsonSerializer.SerializeToUtf8Bytes(Json);
            await channel.Publish("TestExchange", String.Empty, false, false, properties, body);
            //while (!channel.IsClosed)
            //{
            //    await channel.Publish("TestExchange", String.Empty, false, false, properties, body);
            //}
            await connection.WaitEndReading();
        }
        public static async ValueTask RunJSONConsumer()
        {
            var address = Dns.GetHostAddresses("centos0.mshome.net")[0];
            RabbitMQConnectionFactoryBuilder builder = new RabbitMQConnectionFactoryBuilder(new IPEndPoint(address, 5672));
            var factory = builder.ConnectionInfo("guest", "guest", "/")
                                 .Heartbeat(60)
                                 .ProductName("AMQP.Client.RabbitMQ")
                                 .ProductVersion("0.0.1")
                                 .ConnectionName("AMQP.Client.RabbitMQ:Test")
                                 .ClientInformation("TEST TEST TEST")
                                 .ClientCopyright("©")
                                 .Build();
            var connection = factory.CreateConnection();
            await connection.StartAsync();
            var channel = await connection.CreateChannel();
            await channel.ExchangeDeclareAsync("TestExchange", ExchangeType.Direct, arguments: new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueDeclareAsync("TestQueue", false, false, false, new Dictionary<string, object> { { "TEST_ARGUMENT", true } });
            await channel.QueueBindAsync("TestQueue", "TestExchange");

            var consumer = channel.CreateChunkedConsumer("TestQueue", "JsonConsumer",noAck:true);            
            JSONObject value;
            consumer.Received += (deliver, result) =>
            {
                var reader = new Utf8JsonReader(result.Chunk, result.IsCompleted, default);
                value = JsonSerializer.Deserialize<JSONObject>(ref reader);
                if (result.IsCompleted)
                {
                    
                }
                //while (reader.Read())
                //{
                //    Console.Write(reader.TokenType);

                //    switch (reader.TokenType)
                //    {
                //        case JsonTokenType.PropertyName:
                //        case JsonTokenType.String:
                //            {
                //                string text = reader.GetString();
                //                Console.Write(" ");
                //                Console.Write(text);
                //                break;
                //            }

                //        case JsonTokenType.Number:
                //            {
                //                int value = reader.GetInt32();
                //                Console.Write(" ");
                //                Console.Write(value);
                //                break;
                //            }

                //            // Other token types elided for brevity
                //    }
                //}
                
            };
            await consumer.ConsumerStartAsync();
            await connection.WaitEndReading();
        }
    }
}
