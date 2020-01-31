namespace AMQP.Client.RabbitMQ.Protocol
{
    public readonly struct RabbitMQConnectionInfo
    {
        public readonly string User;
        public readonly string Password;
        public readonly string VHost;
        public RabbitMQConnectionInfo(string user,string password,string vhost)
        {
            User = user;
            Password = password;
            VHost = vhost;
        }
        public static RabbitMQConnectionInfo DefaultConnectionInfo()
        {
            return new RabbitMQConnectionInfo("guest", "guest", "/");
        }
    }
}
