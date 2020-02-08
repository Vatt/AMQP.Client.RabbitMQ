using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    public class RabbitMQChannel : RabbitMQChannelReaderWriter,IRabbitMQChannel
    {
        
        private readonly short _channelId;
        private bool _isOpen;
        private bool _started;

        private readonly RabbitMQProtocol _protocol;
        public short ChannelId => _channelId;
        public bool IsOpen => _isOpen;
        public RabbitMQChannel(RabbitMQProtocol protocol,short id) : base(protocol)
        {
            _channelId = id;
            _protocol = protocol;
        }



        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(header.Chanell == _channelId);
            _isOpen = await ReadChannelOpenOk();
            _started = true;
        }

        public ValueTask<bool> TryCloseChannelAsync()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> TryOpenChannelAsync()
        {
            await SendChannelOpen(_channelId);
            await Task.Run(() => { while (!_started) { } });
            return _isOpen;
        }
    }
}
