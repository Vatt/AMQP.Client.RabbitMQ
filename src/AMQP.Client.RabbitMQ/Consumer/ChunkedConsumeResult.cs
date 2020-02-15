using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace AMQP.Client.RabbitMQ.Consumer
{
    public readonly struct ChunkedConsumeResult
    {
        public readonly ReadOnlySequence<byte> Chunk;
        public readonly bool IsCompleted;
        public ChunkedConsumeResult(ReadOnlySequence<byte> chunk, bool isCompleted)
        {
            Chunk = chunk;
            IsCompleted = isCompleted;
        }
    }
}
