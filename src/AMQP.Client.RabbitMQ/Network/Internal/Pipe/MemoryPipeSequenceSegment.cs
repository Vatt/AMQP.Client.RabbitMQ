using System.Buffers;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pipe
{
    partial class MemoryPipe
    {
        internal class MemoryPipeSequenceSegment : ReadOnlySequenceSegment<byte>
        {
            public MemoryPipeSequenceSegment(MemoryPipeBlock block)
            {
                Memory = block.Readable;
            }

            public static MemoryPipeSequenceSegment Create(MemoryPipeBlock block) => new  MemoryPipeSequenceSegment(block);

            public static (MemoryPipeSequenceSegment, MemoryPipeSequenceSegment) Create(MemoryPipeBlock block0, MemoryPipeBlock block1)
            {
                var first = Create(block0);
                var last = Create(block1);
                first.Next = last;
                return (first, last);
            }
        }
    }
}