using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pipe
{
    partial class MemoryPipe
    {
        private class MemoryPipeBlock : IDisposable
        {
            private IMemoryOwner<byte> _data;
            private Memory<byte> _dataMemory;
            private readonly int _dataLength;
            internal int _writerIndex;
            internal int _readerIndex;
            private bool _isDisposed;
            public Memory<byte> Writable => _dataMemory.Slice(_writerIndex, _dataLength - _writerIndex);
            public Memory<byte> Readable => _dataMemory.Slice(_readerIndex, _writerIndex - _readerIndex);
            public bool WriterComplete { get; private set; }
            public bool ReaderComplete { get; private set; }
            public bool IsCompleted => WriterComplete && ReaderComplete;



            private MemoryPipeBlock(IMemoryOwner<byte> data)
            {
                _data = data;
                _dataMemory = _data.Memory;
                WriterComplete = false;
                ReaderComplete = false;
                _writerIndex = 0;
                _readerIndex = 0;
                _dataLength = _data.Memory.Length;
                _isDisposed = false;
            }

            public void AdvanceReaderPosition(int bytes)
            {
                _readerIndex += bytes;
                if (_readerIndex > _writerIndex)
                {
                    MemoryPipeThrowHelper.ThrowIndexOutOfRangeException(MemoryPipeThrowHelper.ExceptionArguments.MemoryPipeBlock);
                }

                if (_readerIndex == _writerIndex && _writerIndex == _dataLength)
                {
                    ReaderComplete = true;
                }
            }

            public void AdvanceWriterPosition(int bytes)
            {
                _writerIndex += bytes;
                if (_writerIndex > _dataLength)
                {
                    MemoryPipeThrowHelper.ThrowIndexOutOfRangeException(MemoryPipeThrowHelper.ExceptionArguments.MemoryPipeBlock);
                }

                if (_writerIndex == _dataLength)
                {
                    WriterComplete = true;
                }
            }
            public void Dispose()
            {
                if (!_isDisposed)
                {
                    return;
                }
                _writerIndex = -1;
                _readerIndex = -1;
                _data?.Dispose();
                _data = null;
                _isDisposed = true;
            }

            public static MemoryPipeBlock Create(IMemoryOwner<byte> data)
            {
                return new MemoryPipeBlock(data);
            }
        }
    }
}