using System.Buffers;
using System.Collections.Concurrent;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pool
{
    public class RabbitMQMemoryPool : MemoryPool<byte>
    {
        private static readonly int BLOCK_SIZE = 128 * 4096;
        private readonly object disposeLock = new object();
        public override int MaxBufferSize => BLOCK_SIZE;
        private readonly ConcurrentStack<MemoryBlock> _blocks;

        private bool _isDisposed;

        public RabbitMQMemoryPool()
        {
            _isDisposed = false;
            _blocks = new ConcurrentStack<MemoryBlock>();
            allocateBlock();
        }
        private void allocateBlock()
        {
            var newPool = MemoryBlock.Create(BLOCK_SIZE, this);
            _blocks.Push(newPool);
        }

        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            if (_isDisposed)
            {
                MemoryPoolThrowHelper.ThrowObjectDisposedException(MemoryPoolThrowHelper.ExceptionArgument.MemoryBlock);
            }
            if (_blocks.TryPop(out var block))
            {
                return block;
            }
            allocateBlock();
            _blocks.TryPop(out block);
            return block;
        }
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            lock (disposeLock)
            {
                while (_blocks.TryPop(out var block))
                {
                    block.Dispose();
                }

                _isDisposed = true;
            }
        }
    }
}
