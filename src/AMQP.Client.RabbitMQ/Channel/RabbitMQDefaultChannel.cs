using AMQP.Client.RabbitMQ.Exchange;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Channel
{
    public class RabbitMQDefaultChannel : ChannelReaderWriter, IRabbitMQDefaultChannel
    {

        private readonly ushort _channelId;
        private bool _isOpen;
        private Action<ushort> _managerCloseCallback;
        private TaskCompletionSource<bool> _openSrc =  new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _closeSrc =  new TaskCompletionSource<bool>();
        private readonly RabbitMQProtocol _protocol;
        public ushort ChannelId => _channelId;
        public bool IsOpen => _isOpen;

        private ExchangeHandler _exchangeMethodHandler;
        internal RabbitMQDefaultChannel(RabbitMQProtocol protocol, ushort id, Action<ushort> closeCallback) : base(protocol)
        {
            _channelId = id;
            _protocol = protocol;
            _isOpen = false;
            _managerCloseCallback = closeCallback;
            _exchangeMethodHandler = new ExchangeHandler(_channelId,_protocol);
        }



        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(header.Channel == _channelId);
            var method = await ReadMethodHeader();
            switch (method.ClassId)
            {
                case 20://Channels class
                    {
                        await HandleMethodAsync(method);
                        break;
                    }
                case 40://Exchange class
                    {
                        await _exchangeMethodHandler.HandleMethodAsync(method);
                        break;
                    }
                default: throw new Exception($"{nameof(RabbitMQDefaultChannel)}.HandleAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");
            }
        }
        public async ValueTask HandleMethodAsync(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 20);
            switch (method.MethodId)
            {
                case 11:
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
                case 41:
                    {
                        var result = await ReadChannelCloseOk();
                        _isOpen = false;
                        _closeSrc.SetResult(_isOpen);
                        break;
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQDefaultChannel)}.HandleMethodAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        public async ValueTask<bool> ExchangeDeclareAsync(string name, string type, bool durable = false, bool autoDelete=false, Dictionary<string, object> arguments = null)
        {
            return await _exchangeMethodHandler.TryDeclareAsync(name, type, durable, autoDelete, arguments);
        }
        public async ValueTask ExchangeDeclareNoWaitAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            await _exchangeMethodHandler.TryDeclareNoWaitAsync(name, type, durable, autoDelete, arguments);
        }
        public async ValueTask<bool> ExchangeDeclarePassiveAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return await _exchangeMethodHandler.TryDeclarePassiveAsync(name, type, durable, autoDelete, arguments);
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
