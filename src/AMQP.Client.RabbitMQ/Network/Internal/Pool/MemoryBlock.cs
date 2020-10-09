using System;
using System.Buffers;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pool
{
    internal class MemoryBlock : IMemoryOwner<byte>
    {
        private RabbitMQMemoryPool _owner;
        private byte[] _pinnedArray;
        internal RabbitMQMemoryPool Owner => _owner as RabbitMQMemoryPool;
        public Memory<byte> Memory { get; }
        private MemoryBlock(byte[] data, MemoryPool<byte> owner)
        {
            _pinnedArray = data;
            Memory = _pinnedArray;
            _owner = owner as RabbitMQMemoryPool;
        }
        public static MemoryBlock Create(int length, MemoryPool<byte> owner)
        {
            var pinnedArray = GC.AllocateUninitializedArray<byte>(length, pinned: true);
            return new MemoryBlock(pinnedArray, owner);
        }

        public void Return()
        {
            _owner.Return(this);
        }
        public void Dispose()
        {
            _pinnedArray = null;
        }
    }
}
