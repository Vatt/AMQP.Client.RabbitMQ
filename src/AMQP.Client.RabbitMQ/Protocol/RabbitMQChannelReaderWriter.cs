using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Protocol
{
    public class RabbitMQChannelReaderWriter
    {
        private RabbitMQProtocol _protocol;
        public RabbitMQChannelReaderWriter(RabbitMQProtocol protocol)
        {
            _protocol = protocol;
        }
        public async ValueTask SendChannelOpen(short channelId)
        {
            await _protocol.Writer.WriteAsync(new ChannelOpenWriter(), channelId);
        }
        public async ValueTask<bool> ReadChannelOpenOk()
        {
            var result = await _protocol.Reader.ReadAsync(new ChannelOpenOkReader());
            _protocol.Reader.Advance();
            return result.Message;
        }
    }
}
