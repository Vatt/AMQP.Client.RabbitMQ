using AMQP.Client.RabbitMQ.Basic;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Exchange;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using AMQP.Client.RabbitMQ.Queue;
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
        private QueueHandler _queueMethodHandler;
        private BasicHandler _basicHandler;
        internal RabbitMQDefaultChannel(RabbitMQProtocol protocol, ushort id, Action<ushort> closeCallback) : base(protocol)
        {
            _channelId = id;
            _protocol = protocol;
            _isOpen = false;
            _managerCloseCallback = closeCallback;
            _exchangeMethodHandler = new ExchangeHandler(_channelId,_protocol);
            _queueMethodHandler = new QueueHandler(_channelId,_protocol);
            _basicHandler = new BasicHandler(_channelId, _protocol);
        }



        public async ValueTask HandleAsync(FrameHeader header)
        {
            Debug.Assert(header.Channel == _channelId);
            switch(header.FrameType)
            {
                case Constants.FrameMethod:
                    {
                        await ProcessMethod();
                        break;
                    }
                    /*
                case Constants.FrameHeader:
                    {
                        await _basicHandler.HandleAsync(header);
                        break;
                    }
                    */
                default: throw new Exception($"Frame type missmatch{nameof(RabbitMQDefaultChannel)}:{header.FrameType}, {header.Channel}, {header.PaylodaSize}");
            }
        }
        public async ValueTask ProcessMethod()
        {
            var method = await ReadMethodHeader();
            Debug.WriteLine($"{method.ClassId} {method.MethodId}");
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
                case 50://queue class
                    {
                        await _queueMethodHandler.HandleMethodAsync(method);
                        break;
                    }
                case 60://basic class
                    {
                        await _basicHandler.HandleMethodHeader(method);
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
        public async ValueTask<bool> ExchangeDeclareAsync(string name, string type, bool durable = false, bool autoDelete=false, Dictionary<string, object> arguments = null)
        {
            return await _exchangeMethodHandler.DeclareAsync(name, type, durable, autoDelete, arguments);
        }
        public async ValueTask ExchangeDeclareNoWaitAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            await _exchangeMethodHandler.DeclareNoWaitAsync(name, type, durable, autoDelete, arguments);
        }
        public async ValueTask<bool> ExchangeDeclarePassiveAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return await _exchangeMethodHandler.DeclarePassiveAsync(name, type, durable, autoDelete, arguments);
        }
        public async Task<bool> TryOpenChannelAsync()
        {
            await SendChannelOpen(_channelId);
            return await _openSrc.Task;
        }



        public async ValueTask<bool> ExchangeDeleteAsync(string name, bool ifUnused = false)
        {
            return await _exchangeMethodHandler.DeleteAsync(name, ifUnused);
        }

        public async ValueTask ExchangeDeleteNoWaitAsync(string name, bool ifUnused = false)
        {
            await _exchangeMethodHandler.DeleteNoWaitAsync(name, ifUnused);
        }

        public async ValueTask<QueueDeclareOk> QueueDeclareAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments)
        {
            return await _queueMethodHandler.DeclareAsync(name, durable, exclusive, autoDelete, arguments);
        }

        public async ValueTask<QueueDeclareOk> QueueDeclarePassiveAsync(string name)
        {
            return await _queueMethodHandler.DeclarePassiveAsync(name);
        }
        public async ValueTask<QueueDeclareOk> QueueDeclareQuorumAsync(string name)
        {
            return await _queueMethodHandler.DeclareQuorumAsync(name);
        }

        public async ValueTask QueueDeclareNoWaitAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments)
        {
            await _queueMethodHandler.DeclareNoWaitAsync(name, durable, exclusive, autoDelete, arguments);
        }

        public async ValueTask<bool> QueueBindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            return await _queueMethodHandler.QueueBindAsync(queueName, exchangeName, routingKey, arguments);
        }

        public async ValueTask QueueBindNoWaitAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            await _queueMethodHandler.QueueBindNoWaitAsync(queueName,exchangeName,routingKey,arguments);
        }
        public async ValueTask<bool> QueueUnbindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            return await _queueMethodHandler.QueueUnbindAsync(queueName, exchangeName, routingKey, arguments);
        }

        public async ValueTask<int> QueuePurgeAsync(string queueName)
        {
            return await _queueMethodHandler.QueuePurgeAsync(queueName);
        }
        public async ValueTask QueuePurgeNoWaitAsync(string queueName)
        {
            await _queueMethodHandler.QueuePurgeNoWaitAsync(queueName);
        }

        public async ValueTask QueueDeleteNoWaitAsync(string queueName, bool ifUnused = false, bool ifEmpty = false)
        {
            await _queueMethodHandler.QueueDeleteNoWaitAsync(queueName, ifUnused, ifEmpty);
        }

        public async ValueTask<int> QueueDeleteAsync(string queueName, bool ifUnused = false, bool ifEmpty = false)
        {
            return await _queueMethodHandler.QueueDeleteAsync(queueName, ifUnused, ifEmpty);
        }

        public async ValueTask<RabbitMQChunkedConsumer> CreateChunkedConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                                              bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return await _basicHandler.CreateChunkedConsumer(queueName, consumerTag, noLocal, noAck, exclusive, arguments);
        }

        public async ValueTask<RabbitMQConsumer> CreateConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                                bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return await _basicHandler.CreateConsumer(queueName, consumerTag, noLocal, noAck, exclusive, arguments);
        }
    }
}
