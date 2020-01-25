using System;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ
{
    public readonly struct RabbitMQClientInfo
    {
        public readonly Dictionary<string, object> Properties;
        public readonly string Mechanism;
        public readonly string Locale;
        public RabbitMQClientInfo(Dictionary<string, object> properties, string mechanism, string locale)
        {
            Properties = properties;
            Mechanism = mechanism;
            Locale = locale;
        }   
        public static RabbitMQClientInfo DefaultClientInfo()
        {
            Dictionary<string, object> props = new Dictionary<string, object>{
                { "product", "AMQP.Client.RabbitMQ" },
                { "version", "0.0.1" },
                { "platform", ".Net Core" },
                { "copyright", "Copyright (c) 2007-2020 Pivotal Software, Inc." },
                { "information","Licensed under the MPL. See https://www.rabbitmq.com/" },                
                { "capabilities", new Dictionary<string,object>
                                    {
                                        { "publisher_confirms", true},
                                        { "exchange_exchange_bindings", true},
                                        { "basic.nack", true},
                                        { "consumer_cancel_notify", true},
                                        { "connection.blocked", true},
                                        { "authentication_failure_close", true},

                                    } 
                },
                { "connection_name","AMQP.Client.RabbitMQ:Test" },
            };
            return new RabbitMQClientInfo(props, "PLAIN","en_US") ;
        }
    }
}
