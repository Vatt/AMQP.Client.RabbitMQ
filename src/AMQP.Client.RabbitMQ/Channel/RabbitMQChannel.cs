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
        private  bool _isOpen;
        private readonly TaskCompletionSource<bool>  _openOkSrc = new TaskCompletionSource<bool>();
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
            var method = await ReadMethod();
            Debug.Assert(method.ClassId == 20);
            switch (method.ClassId)
            {
                case 20 when method.MethodId == 11:
                    {
                        _isOpen = await ReadChannelOpenOk();
                        _openOkSrc.SetResult(_isOpen);
                        break;
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQChannel0)}:cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }

        }

        public Task<bool> TryCloseChannelAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> TryOpenChannelAsync()
        {
            
            await SendChannelOpen(_channelId);
            return await _openOkSrc.Task;

            
        }
    }
}
