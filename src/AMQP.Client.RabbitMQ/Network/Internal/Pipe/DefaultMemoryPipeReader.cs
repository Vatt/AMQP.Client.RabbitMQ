using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pipe
{
    partial class MemoryPipe
    {
        internal class DefaultMemoryPipeReader : PipeReader , IValueTaskSource<ReadResult>
        {
            private readonly MemoryPipe _pipe;
            private int _localReadBytes;
            public DefaultMemoryPipeReader(MemoryPipe pipe)
            {
                _pipe = pipe;
                _localReadBytes = 0;
            }
            public override void AdvanceTo(SequencePosition consumed)
            {
                //_localReadBytes += consumed.GetInteger();
                _pipe.ReaderAdvance(consumed.GetInteger());
            }

            public override void AdvanceTo(SequencePosition consumed, SequencePosition examined)
            {
                AdvanceTo(consumed);
            }

            public override void CancelPendingRead()
            {
                throw new NotImplementedException();
            }

            public override void Complete(Exception exception = null)
            {
                throw new NotImplementedException();
            }

            public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
            {
                return _pipe.ReadAsync(cancellationToken);
            }

            public override bool TryRead(out ReadResult result)
            {
                throw new NotImplementedException();
            }

            public ReadResult GetResult(short token) => _pipe.GetReadAsyncResult();

            public ValueTaskSourceStatus GetStatus(short token) => _pipe.GetReadAsyncStatus();

            public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            {
                _pipe.OnReadingComplete(continuation,state,token,flags);
                //throw new NotImplementedException();
            }
        }
    }
}