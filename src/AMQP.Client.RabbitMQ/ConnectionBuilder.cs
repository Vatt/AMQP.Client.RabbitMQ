using AMQP.Client.RabbitMQ.Protocol.Info;
using System.Net;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQConnectionBuilder
    {
        public RabbitMQConnectionInfo ConnInfo;
        public RabbitMQClientInfo ClientInfo;
        public RabbitMQMainInfo MainInfo;
        public IPEndPoint Endpoint;
        public RabbitMQConnectionBuilder(IPEndPoint endpoint)
        {
            ConnInfo = RabbitMQConnectionInfo.DefaultConnectionInfo();
            ClientInfo = RabbitMQClientInfo.DefaultClientInfo();
            MainInfo = RabbitMQMainInfo.DefaultConnectionInfo();
            Endpoint = endpoint;
        }
        public RabbitMQConnectionBuilder ConnectionInfo(string user, string password, string host)
        {
            ConnInfo = new RabbitMQConnectionInfo(user, password, host);
            return this;
        }
        public RabbitMQConnectionBuilder ChanellMax(short chanellMax)
        {
            MainInfo.ChanellMax = chanellMax;
            return this;
        }
        public RabbitMQConnectionBuilder FrameMax(int frameMax)
        {
            MainInfo.FrameMax = frameMax;
            return this;
        }
        public RabbitMQConnectionBuilder Heartbeat(short heartbeat)
        {
            MainInfo.Heartbeat = heartbeat;
            return this;
        }
        public RabbitMQConnectionBuilder ConnectionName(string name)
        {
            ClientInfo.Properties["connection_name"] = name;
            return this;
        }
        public RabbitMQConnectionBuilder ProductName(string name)
        {
            ClientInfo.Properties["product"] = name;
            return this;
        }
        public RabbitMQConnectionBuilder ProductVersion(string version)
        {
            ClientInfo.Properties["version"] = version;
            return this;
        }
        public RabbitMQConnectionBuilder ClientInformation(string name)
        {
            ClientInfo.Properties["information"] = name;
            return this;
        }
        public RabbitMQConnectionBuilder ClientCopyright(string copyright)
        {
            ClientInfo.Properties["copyright"] = copyright;
            return this;
        }
        public RabbitMQConnection Build()
        {
            return new RabbitMQConnection(this);
        }
    }
}
