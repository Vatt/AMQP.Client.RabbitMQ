namespace AMQP.Client.RabbitMQ.Handlers
{
    /*
    internal class BasicHandler
    {
        private Dictionary<string, ConsumerBase> _consumers;
        private TaskCompletionSource<bool> _commonSrc;
        private readonly SemaphoreSlim _semaphore;
        private readonly SemaphoreSlim _writerSemaphore;
        //private readonly ShortStrPayloadReader _shortStrPayloadReader = new ShortStrPayloadReader();
        protected readonly RabbitMQProtocolWriter _protocol;
        protected readonly ushort _channelId;
        public BasicHandler(ushort channelId, RabbitMQProtocolWriter protocol, SemaphoreSlim writerSemaphore)
        {
            _consumers = new Dictionary<string, ConsumerBase>();
            _semaphore = new SemaphoreSlim(1);
            _writerSemaphore = writerSemaphore;
            _channelId = channelId;
            _protocol = protocol;

        }
        public async ValueTask HandleMethodHeader(MethodHeader header)
        {
            Debug.Assert(header.ClassId == 60);
            switch (header.MethodId)
            {
                case 60://deliver method
                    {
                        var deliver = await _protocol.ReadBasicDeliverAsync().ConfigureAwait(false);
                        if (!_consumers.TryGetValue(deliver.ConsumerTag, out var consumer))
                        {
                            throw new Exception($"{nameof(BasicHandler)}: cant signal to consumer");
                        }
                        await consumer.Delivery(deliver).ConfigureAwait(false);
                        break;
                    }
                case 21:// consume-ok 
                    {
                        var result = await _protocol.ReadBasicConsumeOkAsync().ConfigureAwait(false);
                        //_consumeOkSrc.SetResult(result);
                        if (!_consumers.TryGetValue(result, out var consumer))
                        {
                            throw new Exception($"{nameof(BasicHandler)}: cant signal consume-ok to consumer. Tag:{result}");
                        }
                        consumer.ConsumeOkSrc.SetResult(result);
                        break;
                    }
                case 11: // qos-ok
                    {
                        _commonSrc.SetResult(await _protocol.ReadBasicQoSOkAsync().ConfigureAwait(false));
                        break;
                    }
                case 31://consumer cancel-ok
                    {
                        var result = await _protocol.ReadShortStrPayload();
                        if (!_consumers.Remove(result, out var consumer))
                        {
                            throw new Exception($"{nameof(BasicHandler)}: cant signal to consumer or consumer already canceled");
                        }

                        consumer.CancelSrc.SetResult(result);
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
        public RabbitMQChunkedConsumer CreateChunkedConsumer(string queueName, string consumerTag, RabbitMQChannel channel, bool noLocal = false, bool noAck = false,
                                                             bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            var info = new ConsumerInfo(queueName, consumerTag, noLocal, noAck, exclusive, false, arguments);
            var consumer = new RabbitMQChunkedConsumer(info, _protocol, channel);
            if (!_consumers.TryAdd(consumerTag, consumer))
            {
                if (!_consumers.TryGetValue(consumerTag, out ConsumerBase existedConsumer))
                {
                    throw new ArgumentException($"{nameof(BasicHandler)}.{nameof(CreateChunkedConsumer)} cant create consumer:{consumerTag}");
                }
                if (existedConsumer is RabbitMQChunkedConsumer)
                {
                    _semaphore.Release();
                    return (RabbitMQChunkedConsumer)existedConsumer;
                }
                else
                {
                    throw new ArgumentException($"{nameof(BasicHandler)}.{nameof(CreateChunkedConsumer)} consumer {consumerTag} already exists but with a different type");
                }
            }
            return consumer;
        }
        public RabbitMQConsumer CreateConsumer(string queueName, string consumerTag, RabbitMQChannel channel, PipeScheduler scheduler, bool noLocal = false, bool noAck = false,
                                                                bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            var info = new ConsumerInfo(queueName, consumerTag, noLocal, noAck, exclusive, false, arguments);
            var consumer = new RabbitMQConsumer(info, _protocol, scheduler, channel);
            if (!_consumers.TryAdd(consumerTag, consumer))
            {
                if (!_consumers.TryGetValue(consumerTag, out ConsumerBase existedConsumer))
                {
                    throw new ArgumentException($"{nameof(BasicHandler)}.{nameof(CreateChunkedConsumer)} cant create consumer:{consumerTag}");
                }
                if (existedConsumer is RabbitMQConsumer)
                {
                    _semaphore.Release();
                    return (RabbitMQConsumer)existedConsumer;
                }
                else
                {
                    throw new ArgumentException($"{nameof(BasicHandler)}.{nameof(CreateChunkedConsumer)} consumer {consumerTag} already exists but with a different type");
                }
            }
            return consumer;
        }

        public async ValueTask QoS(int prefetchSize, ushort prefetchCount, bool global)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            _commonSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new QoSInfo(prefetchSize, prefetchCount, global);
            await _protocol.SendBasicQoSAsync(_channelId, ref info).ConfigureAwait(false);
            var result = await _commonSrc.Task;
            _semaphore.Release();
        }
    }
    */
}
