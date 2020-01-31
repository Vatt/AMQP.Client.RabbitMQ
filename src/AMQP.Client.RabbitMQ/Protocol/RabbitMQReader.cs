using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.MethodReaders;
using Bedrock.Framework.Protocols;
using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQReader : IAsyncDisposable
    {
        private readonly ProtocolReader _protocol;
        private readonly FrameHeaderReader _headerReader;
        private readonly MethodHeaderReader _methodHeaderReader;
        private readonly RabbitMQMethods _methods;
        private readonly CancellationToken _cancelled;
        public RabbitMQReader(PipeReader pipeReader, CancellationToken token = default)
        {
            _protocol = new ProtocolReader(pipeReader);
            _headerReader = new FrameHeaderReader();
            _methodHeaderReader = new MethodHeaderReader();
            _methods = new RabbitMQMethods(_protocol,token);
            _cancelled = token;
        }
        public async ValueTask StartReading()
        {
            while(true)
            {
                var header = await _protocol.ReadAsync(_headerReader, _cancelled);
                if (header.IsCompleted)
                {
                    break;
                }
                switch(header.Message.FrameType)
                {
                    case 1:
                        {
                            var method = await _protocol.ReadAsync(_methodHeaderReader, _cancelled);
                            await ProcessMethod(method.Message);
                            break;
                        }
                    case 8:
                        {
                            break;
                        }
                }
            }
        }
        public async ValueTask ProcessMethod(MethodHeader method)
        {
            await _methods.HandleMethod(method);
        }
        public async ValueTask DisposeAsync()
        {
            await _protocol.DisposeAsync();
        }
    }
}
