using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Info.Exchange;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Exchange
{
    internal class ExchangeHandler : ExchangeReaderWriter,IMethodHandler
    {
        private Dictionary<string, ExchangeInfo> _declared;
        public ExchangeHandler(RabbitMQProtocol protocol):base(protocol)
        {
            _declared = new Dictionary<string, ExchangeInfo>();
        }
        public async ValueTask HandleMethodAsync(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 40);
            switch (method.MethodId)
            {
                case 11: //declare-ok
                    {
                        var result = await ReadExchangeDeclareOk();
                        break;
                    }
                case 41:
                    {
                        break;
                    }
                default:
                    throw new Exception($"{nameof(ExchangeHandler)}.HandleMethodAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
    }
}
