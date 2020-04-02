namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public readonly struct ConnectionConf
    {
        public readonly string User;
        public readonly string Password;
        public readonly string VHost;
        public ConnectionConf(string user, string password, string vhost)
        {
            User = user;
            Password = password;
            VHost = vhost;
        }
        public static ConnectionConf DefaultConnectionInfo()
        {
            return new ConnectionConf("guest", "guest", "/");
        }
    }
}
