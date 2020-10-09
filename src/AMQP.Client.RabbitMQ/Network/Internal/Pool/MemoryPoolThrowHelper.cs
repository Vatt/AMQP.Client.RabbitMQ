using System;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pool
{
    internal static class MemoryPoolThrowHelper
    {
        public static void ThrowObjectDisposedException(ExceptionArgument argument)
        {
            throw new ObjectDisposedException(argument.ToString());
        }
        public static void ThrowInvalidOperationException(ExceptionArgument argument)
        {
            throw new InvalidOperationException(argument.ToString());
        }
        public enum ExceptionArgument
        {
            MemoryPool,
            MemoryBlock,
        }
    }
}