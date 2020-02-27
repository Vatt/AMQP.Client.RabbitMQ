using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Basic
{
    internal class BasicHandler : BasicReaderWriter
    {
        private Dictionary<string, ConsumerBase> _consumers;
        private TaskCompletionSource<string> _consumerCreateSrc;
        private SemaphoreSlim  _semaphore;
        private SemaphoreSlim  _writerSemaphore;
        public BasicHandler(ushort channelId,RabbitMQProtocol protocol, SemaphoreSlim writerSemaphore) :base(channelId,protocol)
        {
            _consumers = new Dictionary<string, ConsumerBase>();
            _semaphore = new SemaphoreSlim(1);
            _writerSemaphore = writerSemaphore;
        }
        public async ValueTask HandleMethodHeader(MethodHeader header)
        {
            Debug.Assert(header.ClassId == 60);
            switch(header.MethodId)
            {
                case 60://deliver method
                    {
                        var deliver = await ReadBasicDeliver().ConfigureAwait(false);
                        if (!_consumers.TryGetValue(deliver.ConsumerTag, out var consumer))
                        {
                            throw new Exception($"{nameof(BasicHandler)}: cant signal to consume");
                        }
                        await consumer.Delivery(deliver).ConfigureAwait(false);
                        break;
                    }
                case 21:// consume-ok 
                    {
                        var result = await ReadBasicConsumeOk().ConfigureAwait(false);
                        _consumerCreateSrc.SetResult(result);
                        break;
                    }
                default: throw new Exception($"{nameof(BasicHandler)}.HandleMethodAsync: cannot read frame (class-id,method-id):({header.ClassId},{header.MethodId})");
            }
        }
        
        public async ValueTask<RabbitMQChunkedConsumer> CreateChunkedConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                                              bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            await _semaphore.WaitAsync();
            _consumerCreateSrc = new TaskCompletionSource<string>();
            await SendBasicConsume(queueName, consumerTag, noLocal, noAck, exclusive, arguments).ConfigureAwait(false);
            var result = await _consumerCreateSrc.Task.ConfigureAwait(false);
            if (result.Equals(consumerTag))
            {
                var consumer = new RabbitMQChunkedConsumer(consumerTag, _protocol,_channelId, _writerSemaphore);
                if(!_consumers.TryAdd(consumerTag, consumer))
                {
                    if (!_consumers.TryGetValue(consumerTag,out ConsumerBase existedConsumer))
                    {
                        throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} cant create consumer:{consumerTag}");
                    }
                    if(existedConsumer  is RabbitMQChunkedConsumer)
                    {
                        return (RabbitMQChunkedConsumer)existedConsumer;
                    }
                    else
                    {
                        throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} consumer {consumerTag} already exists but with a different type");
                    }
                }
                return consumer;
            }
            else
            {
                throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} : {consumerTag}");
            }
        }
        public async ValueTask<RabbitMQConsumer> CreateConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                                bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            _consumerCreateSrc = new TaskCompletionSource<string>();
            await SendBasicConsume(queueName, consumerTag, noLocal, noAck, exclusive, arguments).ConfigureAwait(false);
            var result = await _consumerCreateSrc.Task.ConfigureAwait(false);
            if (result.Equals(consumerTag))
            {
                var consumer = new RabbitMQConsumer(consumerTag, _protocol, _channelId, _writerSemaphore);
                if (!_consumers.TryAdd(consumerTag, consumer))
                {
                    if (!_consumers.TryGetValue(consumerTag, out ConsumerBase existedConsumer))
                    {
                        throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} cant create consumer:{consumerTag}");
                    }
                    if (existedConsumer is RabbitMQConsumer)
                    {
                        return (RabbitMQConsumer)existedConsumer;
                    }
                    else
                    {
                        throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} consumer {consumerTag} already exists but with a different type");
                    }
                }
                return consumer;
            }
            else
            {
                throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} : {consumerTag}");
            }
        }

    }
}
