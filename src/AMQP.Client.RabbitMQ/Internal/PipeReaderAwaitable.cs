using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal class PipeReaderAwaitable : ICriticalNotifyCompletion
    {

        private static readonly Action _callbackCompleted = () => { };
        private Action _callback;

        public PipeReaderAwaitable GetAwaiter() => this;

        public bool IsCompleted => ReferenceEquals(_callback, _callbackCompleted);
        public ReadOnlySequence<byte> GetResult()
        {
            return default;
        }
        public void OnCompleted(Action continuation)
        {
            if (ReferenceEquals(_callback,_callbackCompleted) ||
                ReferenceEquals(Interlocked.CompareExchange(ref _callback,continuation,null),_callbackCompleted))
            {
                Task.Run(continuation);
                
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }
        public void Complete()
        {
            var continuation = Interlocked.Exchange(ref _callback, _callbackCompleted);
            OnCompleted(continuation);
        }
    }
}
