using System;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pipe
{
    partial class MemoryPipe
    {
        class DefaultMemoryPipeWriter : PipeWriter
        {
            private MemoryPipe _pipe;
            private int _localWritten;
            public DefaultMemoryPipeWriter(MemoryPipe pipe)
            {
                _pipe = pipe;
                _localWritten = 0;
            }

            public override void Advance(int bytes)
            {
                _localWritten += bytes;
            }

            public override Memory<byte> GetMemory(int sizeHint = 0) => _pipe.WritableMemory;
            public override Span<byte> GetSpan(int sizeHint = 0) => _pipe.WritableMemory.Span;

            public override void CancelPendingFlush()
            {
                throw new NotImplementedException();
            }

            public override void Complete(Exception exception = null)
            {
                throw new NotImplementedException();
            }

            public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = new CancellationToken())
            {
                Debug.Assert(_localWritten > 0);
                _pipe.WriterAdvance(_localWritten);
                _localWritten = 0;
                return new ValueTask<FlushResult>(new FlushResult(false, false));
            }

            public override ValueTask<FlushResult> WriteAsync(ReadOnlyMemory<byte> source,
                CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException("Pls dont use this method");
            }
        }
    }

}