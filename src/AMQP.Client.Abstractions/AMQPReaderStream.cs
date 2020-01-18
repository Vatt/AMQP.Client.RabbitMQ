using System.IO.Pipelines;

namespace AMQP.Client.Abstractions
{
    public abstract class AMQPReaderStream
    {
        private readonly object _lockObj = new object();
        private PipeReader _pipeReader;
        public AMQPReaderStream(AMQPConnection connection)
        {
            _pipeReader = connection.Transport.Input;
        }
    }
}
