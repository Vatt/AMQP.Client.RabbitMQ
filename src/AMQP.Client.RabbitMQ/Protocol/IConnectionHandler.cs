using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public interface IConnectionHandler
    {
        ValueTask OnStartAsync(ServerConf conf);
        ValueTask OnTuneAsync(TuneConf conf);
        ValueTask OnOpenOkAsync();
        ValueTask OnCloseAsync(CloseInfo conf);
        ValueTask OnCloseOkAsync();
        ValueTask OnHeartbeatAsync();
    }
}