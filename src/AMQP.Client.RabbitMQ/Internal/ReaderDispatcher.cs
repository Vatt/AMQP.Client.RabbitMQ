using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal delegate Task ReaderDelegate(ReaderContext ctx);
    internal class ReaderDispatcher:IDisposable
    {
        private readonly object _obj = new object();
        private Queue<Func<ReadOnlySequence<byte>, SequencePosition>> _waiting;
        public ReaderDispatcher()
        {
            _waiting = new Queue<Func<ReadOnlySequence<byte>, SequencePosition>>();
        }
        public void OnPipeReader(ReadOnlySequence<byte> sequence, out SequencePosition position)
        {
            if (_waiting.TryDequeue(out Func<ReadOnlySequence<byte>, SequencePosition> waitingCallback))
            {
                position = waitingCallback(sequence);
                return;
            }
            position = default;
        }
        public void SetWait(Func<ReadOnlySequence<byte>, SequencePosition> callback)
        {
            lock(_obj)
            {
                _waiting.Enqueue(callback);
            }
            
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
