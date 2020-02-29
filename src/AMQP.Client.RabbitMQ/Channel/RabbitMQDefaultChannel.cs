using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Handlers;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using AMQP.Client.RabbitMQ.Protocol.Methods.Channel;
using AMQP.Client.RabbitMQ.Protocol.Methods.Connection;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using AMQP.Client.RabbitMQ.Publisher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        public ushort ChannelId => _channelId;
        public bool IsOpen => _isOpen;
        private RabbitMQMainInfo _mainInfo;
        private readonly SemaphoreSlim _writerSemaphore;
        private ExchangeHandler _exchangeMethodHandler;
        private QueueHandler _queueMethodHandler;
        private BasicHandler _basicHandler;
        internal RabbitMQDefaultChannel(RabbitMQProtocol protocol, ushort id, RabbitMQMainInfo info, Action<ushort> closeCallback) : base(protocol)
        {
            _channelId = id;
            _protocol = protocol;
            _isOpen = false;
            _managerCloseCallback = closeCallback;
            _mainInfo = info;
            _writerSemaphore = new SemaphoreSlim(1);
            _exchangeMethodHandler = new ExchangeHandler(_channelId,_protocol);
            _queueMethodHandler = new QueueHandler(_channelId,_protocol);
            _basicHandler = new BasicHandler(_channelId, _protocol, _writerSemaphore);
        }



        public async ValueTask HandleFrameHeaderAsync(FrameHeader header)
        {
            Debug.Assert(header.Channel == _channelId);
            switch(header.FrameType)
            {
                case Constants.FrameMethod:
                    {
                        await ProcessMethod().ConfigureAwait(false); 
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
            var method = await ReadMethodHeader().ConfigureAwait(false);
            //Debug.WriteLine($"{method.ClassId} {method.MethodId}");
            switch (method.ClassId)
            {
                case 20://Channels class
                    {
                        await HandleMethodAsync(method).ConfigureAwait(false);
                        break;
                    }
                case 40://Exchange class
                    {
                        await _exchangeMethodHandler.HandleMethodAsync(method).ConfigureAwait(false);
                        break;
                    }
                case 50://queue class
                    {
                        await _queueMethodHandler.HandleMethodAsync(method).ConfigureAwait(false);
                        break;
                    }
                case 60://basic class
                    {
                        await _basicHandler.HandleMethodHeader(method).ConfigureAwait(false);
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
                        _isOpen = await ReadChannelOpenOk().ConfigureAwait(false);
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
                        var result = await ReadChannelCloseOk().ConfigureAwait(false);
                        _isOpen = false;
                        _closeSrc.SetResult(_isOpen);
                        break;
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQDefaultChannel)}.HandleMethodAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        public async Task<bool> TryOpenChannelAsync()
        {
            await SendChannelOpen(_channelId).ConfigureAwait(false);
            return await _openSrc.Task.ConfigureAwait(false);
        }
        public async Task<bool> TryCloseChannelAsync(string reason)
        {
            await SendChannelClose(new CloseInfo(ChannelId, Constants.ReplySuccess, reason, 20, 41)).ConfigureAwait(false);
            var result = await _closeSrc.Task.ConfigureAwait(false);
            _managerCloseCallback(_channelId);
            return result;
        }
        public async Task<bool> TryCloseChannelAsync(short replyCode, string replyText, short failedClassId, short failedMethodId)
        {
            await SendChannelClose(new CloseInfo(_channelId, replyCode, replyText, failedClassId, failedMethodId)).ConfigureAwait(false);
            var result = await _closeSrc.Task.ConfigureAwait(false);
            _managerCloseCallback(_channelId);
            return result;
        }
        private void CloseChannel()
        {
            
        }

        public ValueTask<bool> ExchangeDeclareAsync(string name, string type, bool durable = false, bool autoDelete=false, Dictionary<string, object> arguments = null)
        {
            return _exchangeMethodHandler.DeclareAsync(name, type, durable, autoDelete, arguments);
        }
        public ValueTask ExchangeDeclareNoWaitAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return _exchangeMethodHandler.DeclareNoWaitAsync(name, type, durable, autoDelete, arguments);
        }
        public ValueTask<bool> ExchangeDeclarePassiveAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
        {
            return _exchangeMethodHandler.DeclarePassiveAsync(name, type, durable, autoDelete, arguments);
        }




        public ValueTask<bool> ExchangeDeleteAsync(string name, bool ifUnused = false)
        {
            return _exchangeMethodHandler.DeleteAsync(name, ifUnused);
        }

        public ValueTask ExchangeDeleteNoWaitAsync(string name, bool ifUnused = false)
        {
            return _exchangeMethodHandler.DeleteNoWaitAsync(name, ifUnused);
        }

        public ValueTask<QueueDeclareOk> QueueDeclareAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments)
        {
            return _queueMethodHandler.DeclareAsync(name, durable, exclusive, autoDelete, arguments);
        }

        public ValueTask<QueueDeclareOk> QueueDeclarePassiveAsync(string name)
        {
            return _queueMethodHandler.DeclarePassiveAsync(name);
        }
        public ValueTask<QueueDeclareOk> QueueDeclareQuorumAsync(string name)
        {
            return _queueMethodHandler.DeclareQuorumAsync(name);
        }

        public ValueTask QueueDeclareNoWaitAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments)
        {
            return _queueMethodHandler.DeclareNoWaitAsync(name, durable, exclusive, autoDelete, arguments);
        }

        public ValueTask<bool> QueueBindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            return _queueMethodHandler.QueueBindAsync(queueName, exchangeName, routingKey, arguments);
        }

        public ValueTask QueueBindNoWaitAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            return _queueMethodHandler.QueueBindNoWaitAsync(queueName,exchangeName,routingKey,arguments);
        }
        public ValueTask<bool> QueueUnbindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            return _queueMethodHandler.QueueUnbindAsync(queueName, exchangeName, routingKey, arguments);
        }

        public ValueTask<int> QueuePurgeAsync(string queueName)
        {
            return _queueMethodHandler.QueuePurgeAsync(queueName);
        }
        public ValueTask QueuePurgeNoWaitAsync(string queueName)
        {
            return _queueMethodHandler.QueuePurgeNoWaitAsync(queueName);
        }

        public ValueTask QueueDeleteNoWaitAsync(string queueName, bool ifUnused = false, bool ifEmpty = false)
        {
            return _queueMethodHandler.QueueDeleteNoWaitAsync(queueName, ifUnused, ifEmpty);
        }

        public ValueTask<int> QueueDeleteAsync(string queueName, bool ifUnused = false, bool ifEmpty = false)
        {
            return _queueMethodHandler.QueueDeleteAsync(queueName, ifUnused, ifEmpty);
        }

        public ValueTask<RabbitMQChunkedConsumer> CreateChunkedConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                                        bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return _basicHandler.CreateChunkedConsumer(queueName, consumerTag, noLocal, noAck, exclusive, arguments);
        }

        public ValueTask<RabbitMQConsumer> CreateConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                            bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return _basicHandler.CreateConsumer(queueName, consumerTag, noLocal, noAck, exclusive, arguments);
        }

        public RabbitMQPublisher CreatePublisher()
        {
            return new RabbitMQPublisher(_channelId, _protocol, _mainInfo.FrameMax, _writerSemaphore);
        }

        public ValueTask QoS(int prefetchSize, ushort prefetchCount, bool global)
        {
            return _basicHandler.QoS(prefetchSize, prefetchCount, global);
        }

        public ValueTask Ack(long deliveryTag, bool multiple = false)
        {
            return _protocol.Writer.WriteAsync(new BasicAckWriter(_channelId), new AckInfo(deliveryTag, multiple));
        }

        public ValueTask Reject(long deliveryTag, bool requeue)
        {
            return _protocol.Writer.WriteAsync(new BasicRejectWriter(_channelId), new RejectInfo(deliveryTag, requeue));
        }

    }
}
