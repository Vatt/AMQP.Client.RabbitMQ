using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System.IO.Pipelines;
using System.Threading;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocol
    {        
        public readonly ProtocolReader Reader;
        public readonly ProtocolWriter Writer;
        public RabbitMQProtocol(ConnectionContext ctx)
        {
            Reader = Bedrock.Framework.Protocols.Protocol.CreateReader(ctx);
            Writer = Bedrock.Framework.Protocols.Protocol.CreateWriter(ctx);
        }

    }
}
