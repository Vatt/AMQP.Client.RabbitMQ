using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pipe
{
    public partial class MemoryPipe
    {
        private readonly MemoryPool<byte> _pool;
        private readonly object _readerLock = new object();
        private readonly ConcurrentQueue<MemoryPipeBlock> _tail;
        private MemoryPipeBlock _readerHead;
        private MemoryPipeBlock _writerHead;
        private PipeAwaitable _awaitable;


        private DefaultMemoryPipeReader _reader;
        private DefaultMemoryPipeWriter _writer;
        private PipeAwaitable _readerAwaitable;
        private MemoryPipeState _state;
        public bool ReadingInProgress => _state.IsReadingActive;

        public Memory<byte> WritableMemory => _writerHead.Writable;
        public Memory<byte> ReadableMemory => _readerHead.Readable;
        internal PipeReader Reader => _reader;
        internal PipeWriter Writer => _writer;
        public MemoryPipe(MemoryPool<byte> pool)
        {
            _pool = pool;
            _writerHead = createBlock();
            _readerHead = _writerHead;
            _state = new MemoryPipeState();
            _tail = new ConcurrentQueue<MemoryPipeBlock>();
            _state.ResetAll();
            //_awaitable = new PipeAwaitable(false, null);

            _writer = new DefaultMemoryPipeWriter(this);
            _reader = new DefaultMemoryPipeReader(this);
        }


        private void ReaderAdvance(int bytes)
        {
           /* if (bytes == 0)
            {
                return;
            }*/
            _readerHead.AdvanceReaderPosition(bytes);
            if (_readerHead.IsCompleted)
            {
                if (!ReferenceEquals(_readerHead, _writerHead))
                {
                    if (_tail.IsEmpty)
                    {
                        _readerHead = _writerHead;
                    }
                    else
                    {
                        _readerHead.Release();
                        _readerHead = null;
                        _ = _tail.TryDequeue(out _readerHead);
                    }
                }
            }
            //if (_state.IsReadingActive)
            //{
            //    _state.CompleteReader();
            //}
            
        }
        private void WriterAdvance(int bytes)
        {
            _writerHead.AdvanceWriterPosition(bytes);

            if (_writerHead.WriterComplete)
            {
                if (!ReferenceEquals(_writerHead, _readerHead))
                {
                    _tail.Enqueue(_writerHead);
                    _writerHead = createBlock();
                }
                else
                {
                    _tail.Enqueue(_writerHead);
                    _writerHead = createBlock();
                }
            }

            //if (_state.IsReadingActive)
            //{
            //    _state.CompleteReader();
            //}           
        }

        private MemoryPipeBlock createBlock()
        {
            return MemoryPipeBlock.Create(_pool.Rent());
        }

        private ValueTaskSourceStatus GetReadAsyncStatus()
        {
            //if (_state.IsReadingActive && _state.IsReadingComplete)
            if (ReadableMemory.Length > 0)
            {
                return ValueTaskSourceStatus.Succeeded;
            }
            return ValueTaskSourceStatus.Pending;
        }
        private ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = default)
        {
            
            if (ReadableMemory.Length > 0)
            {
                var headSegment = MemoryPipeSequenceSegment.Create(_readerHead);
                var sequence = new ReadOnlySequence<byte>(headSegment,0,headSegment, headSegment.Memory.Length);
                return new ValueTask<ReadResult>(new ReadResult(sequence, false, false ));
            }
            // _state.SetReadingState();
           
            return new ValueTask<ReadResult>(_reader, token:0);
        }

        private ReadResult GetReadAsyncResult()
        {
            // while (true)
            // {
            //     if (ReadableMemory.Length > 0)
            //     {
            //         break;
            //     }
            // }
            var headSegment = MemoryPipeSequenceSegment.Create(_readerHead);
            var sequence = new ReadOnlySequence<byte>(headSegment,0,headSegment, headSegment.Memory.Length);
            return new ReadResult(sequence, false, false );
        }

        private async void OnReadingComplete(Action<object?> continuation, object? state, short token,
            ValueTaskSourceOnCompletedFlags flags)
        {
            ThreadPool.QueueUserWorkItem(continuation, state, preferLocal: true);
        }
    }
}