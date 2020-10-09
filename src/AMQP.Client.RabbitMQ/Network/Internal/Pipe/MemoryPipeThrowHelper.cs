using System;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pipe
{
    internal static class MemoryPipeThrowHelper
    {
        internal enum ExceptionArguments
        {
            ReadingIsInProgress,
            MemoryPipeBlock,
            MemoryPipe,
        }
        public static void ThrowIndexOutOfRangeException(ExceptionArguments arg)
        {
            throw new IndexOutOfRangeException(arg.ToString());
        }

        public static void ThrowObjectDisposedException(ExceptionArguments arg)
        {
            throw new ObjectDisposedException(arg.ToString());
        }

        public static void ThrowInvalidOperationException_ReadingIsInProgress()
        {
            throw new InvalidOperationException(ExceptionArguments.ReadingIsInProgress.ToString());
        }
    }
}