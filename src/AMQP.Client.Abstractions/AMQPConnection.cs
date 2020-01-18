using System.IO.Pipelines;
using System.Net;
using Microsoft.AspNetCore.Connections;

namespace AMQP.Client.Abstractions
{
    public abstract class AMQPConnection
    {
        private readonly object _lockObj = new object();
        protected readonly ConnectionContext _context;
        public EndPoint RemoteEndPoint => _context.RemoteEndPoint;
        public IDuplexPipe Transport => _context.Transport;
        public readonly int Chanell;
        public readonly AMQPApiVersion ApiVersion;
        public  AMQPConnection(ConnectionContext context, AMQPApiVersion apiVersion)
        {
            _context = context;
            ApiVersion = apiVersion;
        }
        public abstract void Start();
        public void CloseConnection()
        {
            lock(_lockObj)
            {
                _context.Abort();
            }
            
        }
    }
}
