using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using System.IO.Pipelines;
using System.Net;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnectionFactoryBuilder
    {
        public RabbitMQConnectionInfo ConnInfo;
        public RabbitMQClientInfo ClientInfo;
        public RabbitMQMainInfo MainInfo;
        public EndPoint Endpoint;
        public PipeScheduler PipeScheduler;
        public RabbitMQConnectionFactoryBuilder(EndPoint endpoint)
        {
            ConnInfo = RabbitMQConnectionInfo.DefaultConnectionInfo();
            ClientInfo = RabbitMQClientInfo.DefaultClientInfo();
            MainInfo = RabbitMQMainInfo.DefaultConnectionInfo();
            Endpoint = endpoint;
            PipeScheduler = PipeScheduler.ThreadPool;
        }
        public RabbitMQConnectionFactoryBuilder ConnectionInfo(string user, string password, string host)
        {
            ConnInfo = new RabbitMQConnectionInfo(user, password, host);
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ChanellMax(ushort chanellMax)
        {
            MainInfo.ChannelMax = chanellMax;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder FrameMax(int frameMax)
        {
            MainInfo.FrameMax = frameMax;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder Heartbeat(short heartbeat)
        {
            MainInfo.Heartbeat = heartbeat;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ConnectionName(string name)
        {
            ClientInfo.Properties["connection_name"] = name;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ProductName(string name)
        {
            ClientInfo.Properties["product"] = name;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ProductVersion(string version)
        {
            ClientInfo.Properties["version"] = version;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ClientInformation(string name)
        {
            ClientInfo.Properties["information"] = name;
            return this;
        }
        public RabbitMQConnectionFactoryBuilder ClientCopyright(string copyright)
        {
            ClientInfo.Properties["copyright"] = copyright;
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
