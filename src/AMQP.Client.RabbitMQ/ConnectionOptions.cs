using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using System.Net;

namespace AMQP.Client.RabbitMQ
{
    public class ConnectionOptions
    {
        public ConnectionConf ConnOptions;
        public ClientConf ClientOptions;
        public TuneConf TuneOptions;
        public readonly EndPoint Endpoint;
        public ConnectionOptions(EndPoint endpoint)
        {
            ConnOptions = ConnectionConf.DefaultConnectionInfo();
            ClientOptions = ClientConf.DefaultClientInfo();
            TuneOptions = TuneConf.DefaultConnectionInfo();
            Endpoint = endpoint;
        }
    }
}
