using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace AMQP.Client.RabbitMQ.Internal
{
    internal readonly struct ReaderContext
    {
        public PipeWriter Writer;
        public PipeReader Reader;
    }
}
