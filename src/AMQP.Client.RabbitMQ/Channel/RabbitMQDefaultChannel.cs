using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Info;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    public class RabbitMQDefaultChannel : ChannelReaderWriter, IRabbitMQChannel
    {

        private readonly short _channelId;
        private bool _isOpen;
        private Action<short> _managerCloseCallback;
        private TaskCompletionSource<bool> _openSrc =  new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _closeSrc =  new TaskCompletionSource<bool>();
        private readonly RabbitMQProtocol _protocol;
        public short ChannelId => _channelId;
        public bool IsOpen => _isOpen;
        internal RabbitMQDefaultChannel(RabbitMQProtocol protocol, short id,Action<short> closeCallback) : base(protocol)
        {
            _channelId = id;
            _protocol = protocol;
            _isOpen = false;
            _managerCloseCallback = closeCallback;
        }



        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(header.Chanell == _channelId);
            var method = await ReadMethodHeader();
            Debug.Assert(method.ClassId == 20);
            switch (method.ClassId)
            {
                case 20 when method.MethodId == 11:
                    {
                        _isOpen = await ReadChannelOpenOk();
                        _openSrc.SetResult(_isOpen);

                        break;
                    }
                /*case 20 when method.MethodId == 40: //close
                    {
                        _isOpen = false;
                        _openCloseSrc.SetResult(_isOpen);
                        break;

                    }
                    */
                case 20 when method.MethodId == 41: 
                    {
                        var result = await ReadChannelCloseOk();
                        _isOpen = false;
                        _closeSrc.SetResult(_isOpen);
                        break;
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQChannel0)}:cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }

        }

        public async Task<bool> TryOpenChannelAsync()
        {
            await SendChannelOpen(_channelId);
            return await _openSrc.Task;
        }

        public async Task<bool> TryCloseChannelAsync(string reason)
        {
            await SendChannelClose(new CloseInfo(ChannelId, Constants.ReplySuccess, reason, 20, 41));
            var result = await _closeSrc.Task;
            _managerCloseCallback(_channelId);
            return result;
        }
        public async Task<bool> TryCloseChannelAsync(short replyCode, string replyText, short failedClassId, short failedMethodId)
        {
            await SendChannelClose(new CloseInfo(_channelId, replyCode, replyText, failedClassId, failedMethodId));
            var result = await _closeSrc.Task;
            _managerCloseCallback(_channelId);
            return result;
        }


    }
}
