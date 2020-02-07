using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.MethodReaders;
using Bedrock.Framework.Protocols;
using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public delegate ValueTask AsyncAction<T>(T data);
    public class RabbitMQReader : IAsyncDisposable
    {
        private readonly ProtocolReader _protocol;
        private readonly FrameHeaderReader _headerReader;
        private readonly MethodHeaderReader _methodHeaderReader;
        private readonly CancellationToken _cancelled;
        internal AsyncAction<RabbitMQServerInfo> OnServerInfoReaded;
        internal AsyncAction<RabbitMQInfo> OnInfoReaded;
        public RabbitMQReader(PipeReader pipeReader, CancellationToken token = default)
        {
            _protocol = new ProtocolReader(pipeReader);
            _headerReader = new FrameHeaderReader();
            _methodHeaderReader = new MethodHeaderReader();
            _cancelled = token;
        }
        public async ValueTask StartReading()
        {
            while(true)
            {
                var header = await _protocol.ReadAsync(_headerReader, _cancelled);
                _protocol.Advance();
                if (header.IsCompleted)
                {
                    break;
                }
                switch(header.Message.FrameType)
                {
                    case 1:
                        {
                            var method = await _protocol.ReadAsync(_methodHeaderReader, _cancelled);
                            _protocol.Advance();
                            if (method.IsCompleted) { break; }                            
                            await HandleMethod(method.Message);
                            break;
                        }
                    case 8:
                        {
                            break;
                        }
                }
            }
        }
        private async ValueTask HandleMethod(MethodHeader header)
        {
            switch (header.ClassId)
            {
                case 10 when header.MethodId == 10:
                    {
                        var serverInfo = await ReadStartMethodAsync();
                        await OnServerInfoReaded(serverInfo);
                        break;
                    }
                    /*
                case 10 when header.MethodId == 30:
                    {
                        var info = await ReadTuneMethodAsync();
                        await OnInfoReaded(info);
                        break;
                    }
                    */
                default:
                    throw new Exception($"RabbitMQReader:cannot read frame (class-id,method-id):({header.ClassId},{header.MethodId}");

            }
        }
        private async ValueTask<RabbitMQServerInfo> ReadStartMethodAsync()
        {
            var result = await _protocol.ReadAsync(new StartMethodReader(), _cancelled);
            if (result.IsCompleted)
            {
                return default;
            }
            _protocol.Advance();
            return result.Message;
        }
        private async ValueTask<RabbitMQInfo> ReadTuneMethodAsync()
        {
            var result = await _protocol.ReadAsync(new TuneMethodReader(), _cancelled);
            if (result.IsCompleted)
            {
                return default;
            }
            _protocol.Advance();
            return result.Message;
        }
        public async ValueTask DisposeAsync()
        {
            await _protocol.DisposeAsync();
        }
    }
}


