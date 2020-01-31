using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.MethodReaders;
using Bedrock.Framework.Protocols;

namespace AMQP.Client.RabbitMQ.Protocol.MethodReaders
{
    public class RabbitMQMethods
    {
        private ProtocolReader _protocol;
        private readonly CancellationToken _cancelled;

        public RabbitMQMethods(ProtocolReader protocol, CancellationToken token = default)
        {
            _protocol = protocol;
            _cancelled = token;
        }
        public async ValueTask HandleMethod(MethodHeader header)
        {
            switch (header.ClassId)
            {
                case 10 when header.MethodId == 10:
                    {
                        var serverInfo = await ReadStartMethodAsync();
                        break;
                    }

                case 10 when header.MethodId == 10:
                    {
                        var info = await ReadTuneMethodAsync();
                        break;
                    }
            }
        }

        public async ValueTask<RabbitMQServerInfo> ReadStartMethodAsync()
        {
            var result = await  _protocol.ReadAsync(new StartMethodReader(), _cancelled);
            if (result.IsCompleted)
            {
                return default;
            }
            return result.Message;
        }
        public async ValueTask<RabbitMQInfo> ReadTuneMethodAsync()
        {
            var result = await _protocol.ReadAsync(new TuneMethodReader(), _cancelled);
            if (result.IsCompleted)
            {
                return default;
            }            
            return result.Message;
        }
    }
}