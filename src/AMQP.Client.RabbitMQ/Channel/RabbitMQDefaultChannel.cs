using AMQP.Client.RabbitMQ.Exchange;
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
    public class RabbitMQDefaultChannel : ChannelReaderWriter, IRabbitMQDefaultChannel//,IMethodHandler,IFrameHandler
    {

        private readonly short _channelId;
        private bool _isOpen;
        private Action<short> _managerCloseCallback;
        private TaskCompletionSource<bool> _openSrc =  new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _closeSrc =  new TaskCompletionSource<bool>();
        private readonly RabbitMQProtocol _protocol;
        public short ChannelId => _channelId;
        public bool IsOpen => _isOpen;

        private ExchangeHandler _exchangeMethodHandler;
        internal RabbitMQDefaultChannel(RabbitMQProtocol protocol, short id, Action<short> closeCallback) : base(protocol)
        {
            _channelId = id;
            _protocol = protocol;
            _isOpen = false;
            _managerCloseCallback = closeCallback;
            _exchangeMethodHandler = new ExchangeHandler(_protocol);
        }



        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(header.Chanell == _channelId);
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
        //public async ValueTask ExchangeDeclare(string name, string type)
        //public async ValueTask ExchangeDeclare(string name, string type, bool passive = false, 
        //                                       bool durable = false, bool autoDelete = false, 
        //                                       bool nowait = false, Dictionary<string, object> args = null)
        //{
        //    //var info = new ExchangeDeclareInfo(name, type, false, false,false, true, new Dictionary<string, object>(){ { "1234", 1 } });
        //    var info = new ExchangeDeclareInfo(ChannelId,name, type, passive, durable, autoDelete, nowait, args);
        //    await new ExchangeReaderWriter(_protocol).WriteExchangeDeclareAsync(info);

        //}
        public ExchangeBuilder Exchange()
        {
            return new ExchangeBuilder(_channelId, _exchangeMethodHandler);
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
