using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol.Common;
using System.IO.Pipelines;
using AMQP.Client.RabbitMQ.Channel;

namespace AMQP.Client.RabbitMQ.Handlers
{
    internal class BasicHandler : BasicReaderWriter
    {
        private Dictionary<string, ConsumerBase> _consumers;
        private TaskCompletionSource<string> _consumeOkSrc;
        private TaskCompletionSource<bool> _commonSrc;
        private readonly SemaphoreSlim _semaphore;
        private readonly SemaphoreSlim _writerSemaphore;
        private readonly ShortStrPayloadReader _shortStrPayloadReader = new ShortStrPayloadReader();

        public BasicHandler(ushort channelId, RabbitMQProtocol protocol, SemaphoreSlim writerSemaphore) : base(channelId, protocol)
        {
            _consumers = new Dictionary<string, ConsumerBase>();
            _semaphore = new SemaphoreSlim(1);
            _writerSemaphore = writerSemaphore;
        }
        public async ValueTask HandleMethodHeader(MethodHeader header)
        {
            Debug.Assert(header.ClassId == 60);
            switch (header.MethodId)
            {
                case 60://deliver method
                    {
                        var deliver = await ReadBasicDeliver().ConfigureAwait(false);
                        if (!_consumers.TryGetValue(deliver.ConsumerTag, out var consumer))
                        {
                            throw new Exception($"{nameof(BasicHandler)}: cant signal to consumer");
                        }
                        await consumer.Delivery(deliver).ConfigureAwait(false);
                        break;
                    }
                case 21:// consume-ok 
                    {
                        var result = await ReadBasicConsumeOk().ConfigureAwait(false);
                        _consumeOkSrc.SetResult(result);
                        break;
                    }
                case 11: // qos-ok
                    {
                        _commonSrc.SetResult(await ReadBasicQoSOk().ConfigureAwait(false));
                        break;
                    }
                case 31://consumer cancel-ok
                    {
                        var result = await _protocol.Reader.ReadAsync(_shortStrPayloadReader).ConfigureAwait(false);
                        _protocol.Reader.Advance();
                        if (result.IsCompleted)
                        {
                            //TODO: сделать чтонибудь
                        }
                        if (!_consumers.Remove(result.Message, out var consumer))
                        {
                            throw new Exception($"{nameof(BasicHandler)}: cant signal to consumer or consumer already canceled");
                        }

                        consumer.CancelSrc.SetResult(result.Message);
                        break;
                    }
                default: throw new Exception($"{nameof(BasicHandler)}.{nameof(HandleMethodHeader)}: cannot read frame (class-id,method-id):({header.ClassId},{header.MethodId})");
            }
        }

        public async ValueTask CloseHandler()
        {
            foreach (var item in _consumers)
            {
                await item.Value.CancelAsync();
            }
            _consumers.Clear();
        }

        public async ValueTask<RabbitMQChunkedConsumer> CreateChunkedConsumer(string queueName, string consumerTag, RabbitMQChannel channel, bool noLocal = false, bool noAck = false,
                                                                              bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            _consumeOkSrc = new TaskCompletionSource<string>();
            await SendBasicConsume(queueName, consumerTag, noLocal, noAck, exclusive, arguments).ConfigureAwait(false);
            var result = await _consumeOkSrc.Task.ConfigureAwait(false);
            if (result.Equals(consumerTag))
            {
                var consumer = new RabbitMQChunkedConsumer(consumerTag, _channelId, _protocol, channel);
                if (!_consumers.TryAdd(consumerTag, consumer))
                {
                    if (!_consumers.TryGetValue(consumerTag, out ConsumerBase existedConsumer))
                    {
                        throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} cant create consumer:{consumerTag}");
                    }
                    if (existedConsumer is RabbitMQChunkedConsumer)
                    {
                        _semaphore.Release();
                        return (RabbitMQChunkedConsumer)existedConsumer;
                    }
                    else
                    {
                        throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} consumer {consumerTag} already exists but with a different type");
                    }
                }
                _semaphore.Release();
                return consumer;
            }
            else
            {
                throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} : {consumerTag}");
            }
        }

        public async ValueTask<RabbitMQConsumer> CreateConsumer(string queueName, string consumerTag, PipeScheduler scheduler, RabbitMQChannel channel, bool noLocal = false, bool noAck = false,
                                                                bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            _consumeOkSrc = new TaskCompletionSource<string>();
            await SendBasicConsume(queueName, consumerTag, noLocal, noAck, exclusive, arguments).ConfigureAwait(false);
            var result = await _consumeOkSrc.Task.ConfigureAwait(false);
            if (result.Equals(consumerTag))
            {
                var consumer = new RabbitMQConsumer(consumerTag, _channelId, _protocol, scheduler, channel);
                if (!_consumers.TryAdd(consumerTag, consumer))
                {
                    if (!_consumers.TryGetValue(consumerTag, out ConsumerBase existedConsumer))
                    {
                        throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} cant create consumer:{consumerTag}");
                    }
                    if (existedConsumer is RabbitMQConsumer)
                    {
                        _semaphore.Release();
                        return (RabbitMQConsumer)existedConsumer;
                    }
                    else
                    {
                        throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} consumer {consumerTag} already exists but with a different type");
                    }
                }
                _semaphore.Release();
                return consumer;
            }
            else
            {
                throw new ArgumentException($"{nameof(BasicReaderWriter)}.{nameof(CreateChunkedConsumer)} : {consumerTag}");
            }
        }

        private void DeleteConsumerPrivate(string tag)
        {
            if (!_consumers.Remove(tag, out var consumer))
            {
                throw new Exception($"{nameof(BasicHandler)}: cant signal to consumer or consumer already canceled");
            }
        }

        public async ValueTask QoS(int prefetchSize, ushort prefetchCount, bool global)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            _commonSrc = new TaskCompletionSource<bool>();
            var info = new QoSInfo(prefetchSize, prefetchCount, global);
            await SendBasicQoS(ref info).ConfigureAwait(false);
            var result = await _commonSrc.Task;
            _semaphore.Release();
        }
    }
}
