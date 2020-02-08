using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQProtocol
    {
        private ConnectionContext _context;
        public readonly ProtocolReader Reader;
        public readonly ProtocolWriter Writer;
        public RabbitMQProtocol(ConnectionContext ctx)
        {
            _context = ctx;
            Reader = Bedrock.Framework.Protocols.Protocol.CreateReader(_context);
            Writer = Bedrock.Framework.Protocols.Protocol.CreateWriter(_context);
        }

    }
}
