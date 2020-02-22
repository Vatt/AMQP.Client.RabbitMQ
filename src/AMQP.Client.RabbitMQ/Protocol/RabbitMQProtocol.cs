using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocol
    {
        public readonly ConnectionContext Context;
        public readonly ProtocolReader Reader;
        public readonly ProtocolWriter Writer;
        public RabbitMQProtocol(ConnectionContext ctx)
        {
            Context = ctx;
            Reader = Bedrock.Framework.Protocols.Protocol.CreateReader(Context);
            Writer = Bedrock.Framework.Protocols.Protocol.CreateWriter(Context);
        }

    }
}
