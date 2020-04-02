namespace AMQP.Client.RabbitMQ.Channel
{
    /*
    public class RabbitMQChannel
    {

        private readonly ushort _channelId;
        private bool _isClosed;
        private readonly int _publishBatchSize = 4;
        private readonly ReadOnlyMemory<byte>[] _publishBatch;
        private TaskCompletionSource<bool> _openSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private TaskCompletionSource<bool> _manualCloseSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private TaskCompletionSource<CloseInfo> _channelCloseSrc = new TaskCompletionSource<CloseInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        public ushort ChannelId => _channelId;
        public bool IsClosed => _isClosed;
        private TuneConf _mainInfo;
        private readonly SemaphoreSlim _writerSemaphore;
        private RabbitMQProtocolWriter _protocol;
        private readonly PipeScheduler _scheduler;

        internal RabbitMQChannel(ushort id, TuneConf info, PipeScheduler scheduler)
        {
            _channelId = id;
            _isClosed = true;
            _scheduler = scheduler;
            _mainInfo = info;
            _publishBatch = new ReadOnlyMemory<byte>[_publishBatchSize];
            _writerSemaphore = new SemaphoreSlim(1);
        }



        public async ValueTask HandleFrameHeaderAsync(FrameHeader header)
        {
            Debug.Assert(header.Channel == _channelId);
            if (header.FrameType == Constants.FrameMethod)
            {
                await ProcessMethod().ConfigureAwait(false);
            }
            else
            {
                throw new Exception($"Frame type missmatch{nameof(RabbitMQChannel)}:{header.FrameType}, {header.Channel}, {header.PaylodaSize}");
            }
        }
        public async ValueTask ProcessMethod()
        {
            var method = await _protocol.ReadMethodHeader().ConfigureAwait(false);
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
                default: throw new Exception($"{nameof(RabbitMQChannel)}.HandleAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");
            }
        }
        public async ValueTask HandleMethodAsync(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 20);
            switch (method.MethodId)
            {
                case 11://open-ok
                    {
                        await _protocol.ReadChannelOpenOkAsync().ConfigureAwait(false);
                        _isClosed = false;
                        _openSrc.SetResult(_isClosed);

                        break;
                    }
                case 40: //close
                    {
                        _isClosed = false;
                        var info = await _protocol.ReadClose().ConfigureAwait(false);
                        await ProcessChannelClose();
                        _channelCloseSrc.SetResult(info);
                        break;

                    }
                case 41://close-ok
                    {
                        var result = await _protocol.ReadCloseOk().ConfigureAwait(false);
                        _isClosed = false;
                        _manualCloseSrc.SetResult(_isClosed);
                        break;
                    }
                default:
                    throw new Exception($"{nameof(RabbitMQChannel)}.HandleMethodAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        public async Task OpenAsync(RabbitMQProtocolWriter protocol)
        {
            _protocol = protocol;
            _exchangeMethodHandler = new ExchangeHandler(_channelId, _protocol);
            _queueMethodHandler = new QueueHandler(_channelId, _protocol);
            _basicHandler = new BasicHandler(_channelId, _protocol, _writerSemaphore);
            await _protocol.SendChannelOpenAsync(_channelId).ConfigureAwait(false);
            await _openSrc.Task.ConfigureAwait(false);
            _isClosed = false;

        }
        public Task<bool> CloseAsync(string reason)
        {
            return CloseAsync(Constants.ReplySuccess, reason, 0, 0);
        }
        public Task<CloseInfo> WaitClose()
        {
            return _channelCloseSrc.Task;
        }
        public async Task<bool> CloseAsync(short replyCode, string replyText, short failedClassId, short failedMethodId)
        {
            await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            await _protocol.SendClose(_channelId, 20, 40, new CloseInfo(replyCode, replyText, failedClassId, failedMethodId)).ConfigureAwait(false);
            var result = await _manualCloseSrc.Task.ConfigureAwait(false);
            await ProcessChannelClose().ConfigureAwait(false);
            _channelCloseSrc.SetResult(new CloseInfo(Constants.Success, replyText, 0, 0));
            _writerSemaphore.Release();
            return result;
        }
        private async ValueTask ProcessChannelClose()
        {
            _isClosed = false;
            await _basicHandler.CloseHandler().ConfigureAwait(false);
            _basicHandler = null;
            _exchangeMethodHandler = null;
            _queueMethodHandler = null;
            _protocol = null;
        }
        public ValueTask<bool> ExchangeDeclareAsync(string name, string type, bool durable = false, bool autoDelete = false, Dictionary<string, object> arguments = null)
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
            return _queueMethodHandler.QueueBindNoWaitAsync(queueName, exchangeName, routingKey, arguments);
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

        public RabbitMQChunkedConsumer CreateChunkedConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                                             bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return _basicHandler.CreateChunkedConsumer(queueName, consumerTag, this, noLocal, noAck, exclusive, arguments);
        }

        public RabbitMQConsumer CreateConsumer(string queueName, string consumerTag, PipeScheduler scheduler, bool noLocal = false, bool noAck = false,
                                               bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return _basicHandler.CreateConsumer(queueName, consumerTag, this, scheduler, noLocal, noAck, exclusive, arguments);
        }

        public RabbitMQConsumer CreateConsumer(string queueName, string consumerTag, bool noLocal = false, bool noAck = false,
                                       bool exclusive = false, Dictionary<string, object> arguments = null)
        {
            return _basicHandler.CreateConsumer(queueName, consumerTag, this, _scheduler, noLocal, noAck, exclusive, arguments);
        }

        public ValueTask QoS(int prefetchSize, ushort prefetchCount, bool global)
        {
            return _basicHandler.QoS(prefetchSize, prefetchCount, global);
        }

        public ValueTask Ack(long deliveryTag, bool multiple = false)
        {
            if (IsClosed)
            {
                throw new Exception($"{nameof(RabbitMQChannel)}.{nameof(Ack)}: channel is canceled");
            }
            var info = new AckInfo(deliveryTag, multiple);
            return _protocol.SendAckAsync(ChannelId, ref info);
        }

        public ValueTask Reject(long deliveryTag, bool requeue)
        {
            if (IsClosed)
            {
                throw new Exception($"{nameof(RabbitMQChannel)}.{nameof(Reject)}: channel is canceled");
            }
            var info = new RejectInfo(deliveryTag, requeue);
            return _protocol.SendRejectAsync(ChannelId, ref info);
        }
        public async ValueTask Publish(string exchangeName, string routingKey, bool mandatory, bool immediate, ContentHeaderProperties properties, ReadOnlyMemory<byte> message)
        {
            if (IsClosed)
            {
                throw new Exception($"{nameof(RabbitMQChannel)}.{nameof(Publish)}: channel is canceled");
            }
            var info = new BasicPublishInfo(exchangeName, routingKey, mandatory, immediate);
            var content = new ContentHeader(60, message.Length, ref properties);
            if (message.Length <= _mainInfo.FrameMax)
            {
                var allinfo = new PublishAllInfo(message, ref info, ref content);
                await _protocol.PublishAllAsync(ChannelId, allinfo).ConfigureAwait(false);
                return;
            }


            await _writerSemaphore.WaitAsync().ConfigureAwait(false);
            int written = 0;
            var partialInfo = new PublishPartialInfo(ref info, ref content);
            await _protocol.PublishPartialAsync(ChannelId, partialInfo).ConfigureAwait(false);
            while (written < content.BodySize)
            {
                int batchCnt = 0;
                while (batchCnt < _publishBatchSize && written < content.BodySize)
                {
                    int writable = Math.Min(_mainInfo.FrameMax, (int)content.BodySize - written);
                    _publishBatch[batchCnt] = message.Slice(written, writable);
                    batchCnt++;
                    written += writable;
                }
                await _protocol.PublishBodyAsync(ChannelId, _publishBatch).ConfigureAwait(false);
                _publishBatch.AsSpan().Fill(ReadOnlyMemory<byte>.Empty);
            }

            _writerSemaphore.Release();
        }

        public void Dispose()
        {
            _writerSemaphore.Dispose();
        }
    }
    */

}
