using AMQP.Client.RabbitMQ.Protocol.Framing;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Common;

namespace AMQP.Client.RabbitMQ.Protocol.Methods.Channel
{
    public class ChannelReaderWriter
    {
        protected RabbitMQProtocol _protocol;
        private readonly ChannelOpenWriter _channelOpenWriter = new ChannelOpenWriter();
        private readonly ChannelOpenOkReader channelOpenOkReader = new ChannelOpenOkReader();
        private readonly MethodHeaderReader _methodHeaderReader = new MethodHeaderReader();
        private readonly NoPayloadReader _noPayloadReader = new NoPayloadReader();
        private readonly CloseReader _closeReader = new CloseReader();

        public ChannelReaderWriter(RabbitMQProtocol protocol)
        {
            _protocol = protocol;
        }

        public ValueTask SendChannelOpen(ushort channelId)
        {
            return _protocol.Writer.WriteAsync(_channelOpenWriter, channelId);
        }

        public async ValueTask<bool> ReadChannelOpenOk()
        {
            var result = await _protocol.Reader.ReadAsync(channelOpenOkReader).ConfigureAwait(false);
            _protocol.Reader.Advance();
            return result.Message;
        }

        public async ValueTask<MethodHeader> ReadMethodHeader()
        {
            var result = await _protocol.Reader.ReadAsync(_methodHeaderReader).ConfigureAwait(false);
            _protocol.Reader.Advance();
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            return result.Message;
        }

        public ValueTask SendChannelClose(ushort channelId, CloseInfo info)
        {
            return _protocol.Writer.WriteAsync(new CloseWriter(channelId,20,40), info);
        }

        public async ValueTask<bool> ReadChannelCloseOk()
        {
            var result = await _protocol.Reader.ReadAsync(_noPayloadReader).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }

        public async ValueTask<CloseInfo> ReadChannelClose()
        {
            var result = await _protocol.Reader.ReadAsync(_closeReader).ConfigureAwait(false);
            if (result.IsCanceled)
            {
                //TODO:  сделать чтонибудь
            }
            _protocol.Reader.Advance();
            return result.Message;
        }
    }
}
