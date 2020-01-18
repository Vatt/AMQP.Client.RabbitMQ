using System.Net;
using System.Threading.Tasks;

namespace AMQP.Client.Abstractions
{
    public abstract class AMQPConnectionBuilder
    {
        protected IPEndPoint _endpoint;
        public AMQPConnectionBuilder(IPEndPoint endpoint)
        {
            _endpoint = endpoint; 
        }
        public abstract Task<AMQPConnection> BuildAsync();
    }
}
