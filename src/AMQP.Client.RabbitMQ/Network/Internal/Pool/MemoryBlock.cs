using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pool
{
    internal class MemoryBlock : IMemoryOwner<byte>
    {
        private MemoryPool<byte> _owner;
        private byte[] _pinnedArray;

        public Memory<byte> Memory { get; }
        private MemoryBlock(byte[] data, MemoryPool<byte> owner)
        {
            _pinnedArray = data;
            Memory = _pinnedArray;
            _owner = owner;
        }
        public static MemoryBlock Create(int length, MemoryPool<byte> owner)
        {
            var pinnedArray = GC.AllocateUninitializedArray<byte>(length, pinned: true);
            return new MemoryBlock(pinnedArray, owner);
        }

        public void Return()
        {

        }
        public void Dispose()
        {
            _pinnedArray = null;
        }
    }
}
