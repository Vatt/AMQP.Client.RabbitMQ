using System;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pool
{
    internal static class MemoryPoolThrowHelper
    {
        public static void ThrowObjectDisposedException(ExceptionArgument argument)
        {
            throw new ObjectDisposedException(argument.ToString());
        }

        public enum ExceptionArgument
        {
            MemoryPool,
            MemoryBlock,
        }
    }
}