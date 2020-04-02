using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using System.IO.Pipelines;
using System.Net;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnectionFactoryBuilder
    {
        public ConnectionOptions Options;
        public PipeScheduler PipeScheduler;
        public RabbitMQConnectionFactoryBuilder(EndPoint endpoint)
        {
            Options = new ConnectionOptions(endpoint);
            PipeScheduler = PipeScheduler.ThreadPool;
        }
        public RabbitMQConnectionFactoryBuilder ConnectionInfo(string user, string password, string host)
        {
            Options.ConnOptions = new ConnectionConf(user, password, host);
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ChanellMax(ushort chanellMax)
        {
            Options.TuneOptions.ChannelMax = chanellMax;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder FrameMax(int frameMax)
        {
            Options.TuneOptions.FrameMax = frameMax;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder Heartbeat(short heartbeat)
        {
            Options.TuneOptions.Heartbeat = heartbeat;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ConnectionName(string name)
        {
            Options.ClientOptions.Properties["connection_name"] = name;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ProductName(string name)
        {
            Options.ClientOptions.Properties["product"] = name;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ProductVersion(string version)
        {
            Options.ClientOptions.Properties["version"] = version;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ClientInformation(string name)
        {
            Options.ClientOptions.Properties["information"] = name;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ClientCopyright(string copyright)
        {
            Options.ClientOptions.Properties["copyright"] = copyright;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder Scheduler(PipeScheduler scheduler)
        {
            PipeScheduler = scheduler;
            return this;
        }

        //public RabbitMQConnection Build()
        //{
        //    return new RabbitMQConnection(this);
        //}
        public RabbitMQConnectionFactory Build()
        {
            return new RabbitMQConnectionFactory(this);
        }
    }
}
