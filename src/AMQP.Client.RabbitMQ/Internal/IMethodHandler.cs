using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal interface IMethodHandler
    {

        ValueTask HandleMethodAsync(MethodHeader method);
    }
}
