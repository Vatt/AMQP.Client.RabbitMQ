using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ
{
    public class RabbitMQClientInfo
    {
        public string Product;
        public string Version;
        public string Platform;
        public string Copyright;
        public string Information;
        public string Mechanism;
        public string Response;
        public string Locale;
        public Dictionary<string, object> capabalites;
        public RabbitMQClientInfo()
        {
            Product = "AMQP.Client.RabbitMQ";
            Version = "0.0.1";
            Platform    = ".Net Core";
            Copyright   = string.Empty;
            Information = string.Empty;
            Mechanism   = string.Empty;
            Response    = string.Empty;
            Locale      = "en_US";
            capabalites = new Dictionary<string, object>() {
                { "publisher_confirms",true },
                { "exchange_exchange_bindings",true },
                { "basic.nack",true },
                { "consumer_cancel_notify",true },
                { "connection.blocked",true },
                { "authentication_failure_close",true },
            };


    }
    }
}
