using System.Collections.Generic;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Connection
{
    public readonly struct RabbitMQServerInfo
    {
        public readonly int Major;
        public readonly int Minor;
        public readonly Dictionary<string, object> Properties;
        public readonly string Mechanisms;
        public readonly string Locales;
        public RabbitMQServerInfo(int major, int minor, Dictionary<string, object> props, string mechanisms, string locales)
        {
            Major = major;
            Minor = minor;
            Properties = props;
            Mechanisms = mechanisms;
            Locales = locales;
        }
    }
}
