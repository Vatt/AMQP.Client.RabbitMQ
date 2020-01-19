using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using AMQP.Client.RabbitMQ.Decoder;
namespace AMQP.Client.RabbitMQ.Internal
{
    internal class ReaderDispatcher:IDisposable
    {
        private readonly object _obj = new object();
        private Queue<Action<ReadOnlySequence<byte>>> _waiting;
        public ReaderDispatcher()
        {
            _waiting = new Queue<Action<ReadOnlySequence<byte>>>();
        }
        public void OnPipeReader(ReadOnlySequence<byte> sequence)
        {
            if (_waiting.TryDequeue(out Action<ReadOnlySequence<byte>> waitingCallback))
            {
                waitingCallback(sequence);
            }

        }
        public void SetWait(Action<ReadOnlySequence<byte>> callback)
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
