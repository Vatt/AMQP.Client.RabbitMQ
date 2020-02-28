using AMQP.Client.RabbitMQ.Protocol.Framing;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Channel
{
    public class ChannelReaderWriter
    {
        protected RabbitMQProtocol _protocol;
        public ChannelReaderWriter(RabbitMQProtocol protocol)
        {
            _protocol = protocol;
        }
        public async ValueTask SendChannelOpen(ushort channelId)
        {
            await _protocol.Writer.WriteAsync(new ChannelOpenWriter(), channelId);
        }
        public async ValueTask<bool> ReadChannelOpenOk()
        {
            var result = await _protocol.Reader.ReadAsync(new ChannelOpenOkReader());
            _protocol.Reader.Advance();
            return result.Message;
        }
        public async ValueTask<MethodHeader> ReadMethodHeader()
        {
            var result = await _protocol.Reader.ReadAsync(new MethodHeaderReader());
            _protocol.Reader.Advance();
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            return result.Message;
        }
        public async ValueTask SendChannelClose(CloseInfo info)
        {
            await _protocol.Writer.WriteAsync(new CloseWriter(), info);
        }
        public async ValueTask<bool> ReadChannelCloseOk()
        {
            var result = await _protocol.Reader.ReadAsync(new NoPayloadReader());
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
    }
}
