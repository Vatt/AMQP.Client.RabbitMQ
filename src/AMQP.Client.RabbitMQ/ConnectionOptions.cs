using System.Net;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;

namespace AMQP.Client.RabbitMQ
{
    public class ConnectionOptions
    {
        public readonly EndPoint Endpoint;
        public ClientConf ClientOptions;
        public ConnectionConf ConnOptions;
        public TuneConf TuneOptions;

        public ConnectionOptions(EndPoint endpoint)
        {
            ConnOptions = ConnectionConf.DefaultConnectionInfo();
            ClientOptions = ClientConf.DefaultClientInfo();
            TuneOptions = TuneConf.DefaultConnectionInfo();
            Endpoint = endpoint;
        }
    }
}