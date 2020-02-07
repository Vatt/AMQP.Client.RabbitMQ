using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.MethodWriters;
using Bedrock.Framework.Infrastructure;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQWriter
    {
        private PipeWriter _writer;
        private readonly CancellationToken _cancelled;
        public RabbitMQWriter(PipeWriter writer, CancellationToken token = default)
        {
            _writer = writer;
            _cancelled = token;
        }
        public void Write(ReadOnlySpan<byte> source)
        {
            var writer = new ValueWriter(_writer);
            writer.Write(source);
            writer.Commit();
        }
        public void WriteStartOk(RabbitMQClientInfo info, RabbitMQConnectionInfo connInfo)
        {
            var writer = new StartOkMethodWriter(connInfo);
            writer.WriteMessage(info,_writer);
        }
        public async ValueTask FlushAsync()
        {
            await _writer.FlushAsync(_cancelled);
        }
        public Memory<byte> Test()
        {
            return _writer.GetMemory();
        }
    }
}
