using System.IO.Pipelines;
using AMQP.Client.RabbitMQ.Network.Internal.Pool;

namespace AMQP.Client.RabbitMQ.Network.Internal.Pipe
{
    public class DuplexPipe : IDuplexPipe
    {
        public PipeReader Input { get; }
        public PipeWriter Output { get; }


        public DuplexPipe(PipeReader reader, PipeWriter writer)
        {
            Input = reader;
            Output = writer;
        }
        public static DuplexPipePair CreateConnectionPair(RabbitMQMemoryPool pool)
        {
            var input = new MemoryPipe(pool);
            var output = new MemoryPipe(pool);

            var transportToApplication = new DuplexPipe(output.Reader, input.Writer);
            var applicationToTransport = new DuplexPipe(input.Reader, output.Writer);

            return new DuplexPipePair(applicationToTransport, transportToApplication);
        }


        public readonly struct DuplexPipePair
        {
            public IDuplexPipe Transport { get; }
            public IDuplexPipe Application { get; }

            public DuplexPipePair(IDuplexPipe transport, IDuplexPipe application)
            {
                Transport = transport;
                Application = application;
            }
        }
    }
}