using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;

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
