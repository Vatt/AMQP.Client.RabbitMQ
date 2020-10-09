using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pipe
{
    public partial class MemoryPipe
    {
        private static class PipeState
        {
            public static readonly int Reading = 1;
            public static readonly int ReadingComplete = 2;
            public static readonly int Writing = 4;
        }
        public struct MemoryPipeState
        {
            private int _state;
            public bool IsReadingActive => (_state & PipeState.Reading) == PipeState.Reading;
            public bool IsReadingComplete => (_state & PipeState.ReadingComplete) == PipeState.ReadingComplete;
            public bool IsWritingActive => (_state & PipeState.Writing) == PipeState.Writing;
            public void ResetAll()
            {
                _state = 0;
            }

            public void ResetReader()
            {
                //Debug.Assert(_state == (PipeState.Reading | PipeState.ReadingComplete));
                _state &= ~PipeState.Reading;
                _state &= ~PipeState.ReadingComplete;
            }
            public void CompleteReader()
            {
                _state |= PipeState.ReadingComplete;
                ResetReader();
            }

            public void ResetWriter()
            {
                _state &= ~PipeState.Writing;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetReadingState()
            {
                if (Interlocked.CompareExchange(
                    ref _state,
                    _state | PipeState.Reading,
                    (_state & (~PipeState.Reading)))
                    != (_state & (~PipeState.Reading)))
                {
                    MemoryPipeThrowHelper.ThrowInvalidOperationException_ReadingIsInProgress();
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetWritingState()
            {
                _state |= PipeState.Writing;
            }
        }
    }
}