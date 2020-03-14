using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Handlers
{
    //засейвить только AutoDelete Exclusive очереди?
    public class QueueHandler: QueueReaderWriter
    {
        private readonly SemaphoreSlim _semafore;
        private Dictionary<string, QueueInfo> _queues;
        private TaskCompletionSource<QueueDeclareOk> _declareOkSrc;
        private TaskCompletionSource<bool> _commonSrc;
        private TaskCompletionSource<int> _purgeOrDeleteSrc;
        public QueueHandler(ushort channelId, RabbitMQProtocol protocol) : base(channelId, protocol)
        {
            _queues = new Dictionary<string, QueueInfo>();
            _semafore = new SemaphoreSlim(1);
            _queues = new Dictionary<string, QueueInfo>();
        }

        public async ValueTask HandleMethodAsync(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 50);
            switch (method.MethodId)
            {
                case 11: //declare-ok
                    {
                       _declareOkSrc.SetResult(await ReadQueueDeclareOk().ConfigureAwait(false));
                        break;
                    }
                case 21://bind-ok
                    {
                        _commonSrc.SetResult(await ReadBindOkUnbindOk().ConfigureAwait(false));
                        break;
                    }
                case 51://unbind-ok
                    {
                        _commonSrc.SetResult(await ReadBindOkUnbindOk().ConfigureAwait(false));
                        break;
                    }
                case 31://purge-ok
                    {
                        _purgeOrDeleteSrc.SetResult(await ReadQueuePurgeOkDeleteOk().ConfigureAwait(false));
                        break;
                    }
                case 41: //delete-ok
                    {
                        _purgeOrDeleteSrc.SetResult(await ReadQueuePurgeOkDeleteOk().ConfigureAwait(false));
                        break;
                    }
                default:
                    throw new Exception($"{nameof(QueueHandler)}.HandleMethodAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        public async ValueTask<QueueDeclareOk> DeclareAsync(string name, bool durable, bool exclusive,bool autoDelete, Dictionary<string, object> arguments)
        {
            await _semafore.WaitAsync().ConfigureAwait(false);
            _declareOkSrc = new TaskCompletionSource<QueueDeclareOk>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new QueueInfo(name, durable, exclusive, autoDelete, arguments: arguments);
            await SendQueueDeclare(info).ConfigureAwait(false);
            var okInfo = await _declareOkSrc.Task.ConfigureAwait(false);
            _queues.Add(okInfo.Name, info);
            _semafore.Release();
            return okInfo;
        }
        public ValueTask DeclareNoWaitAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments)
        {
            var info = new QueueInfo(name, durable, exclusive, autoDelete, nowait:true, arguments: arguments);
            return SendQueueDeclare(info);
        }
        public async ValueTask<QueueDeclareOk> DeclarePassiveAsync(string name)
        {
            await _semafore.WaitAsync().ConfigureAwait(false);
            _declareOkSrc = new TaskCompletionSource<QueueDeclareOk>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new QueueInfo(name);
            await SendQueueDeclare(info).ConfigureAwait(false);
            var okInfo = await _declareOkSrc.Task.ConfigureAwait(false);
            _queues.Add(okInfo.Name, info);
            _semafore.Release();
            return okInfo;
        }
        public async ValueTask<QueueDeclareOk> DeclareQuorumAsync(string name)
        {
            await _semafore.WaitAsync().ConfigureAwait(false);
            _declareOkSrc = new TaskCompletionSource<QueueDeclareOk>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new QueueInfo(name,true, arguments:new Dictionary<string, object> {{ "x-queue-type", "quorum" }});
            await SendQueueDeclare(info).ConfigureAwait(false);
            var okInfo = await _declareOkSrc.Task.ConfigureAwait(false);
            _queues.Add(okInfo.Name, info);
            _semafore.Release();
            return okInfo;
        }
        public async ValueTask<bool> QueueBindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            await _semafore.WaitAsync().ConfigureAwait(false);
            _commonSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new QueueBindInfo(queueName, exchangeName, routingKey, false, arguments);
            await SendQueueBind(info).ConfigureAwait(false);
            var result = await _commonSrc.Task.ConfigureAwait(false);
            _semafore.Release();
            return result;
        }

        public ValueTask QueueBindNoWaitAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            var info = new QueueBindInfo(queueName, exchangeName, routingKey, true, arguments);
            return SendQueueBind(info);
        }
        public async ValueTask<bool> QueueUnbindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            await _semafore.WaitAsync().ConfigureAwait(false);
            _commonSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new QueueUnbindInfo(queueName, exchangeName, routingKey, arguments);
            await SendQueueUnbind(info).ConfigureAwait(false);
            var result = await _commonSrc.Task.ConfigureAwait(false);
            _semafore.Release();
            return result;
        }
        public async ValueTask<int> QueuePurgeAsync(string queueName)
        {
            await _semafore.WaitAsync().ConfigureAwait(false);
            _purgeOrDeleteSrc = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new QueuePurgeInfo(queueName, false);
            await SendQueuePurge(info).ConfigureAwait(false);
            var result = await _purgeOrDeleteSrc.Task.ConfigureAwait(false);
            _semafore.Release();
            return result;
        }

        public ValueTask QueuePurgeNoWaitAsync(string queueName)
        {
            var info = new QueuePurgeInfo(queueName, true);
            return SendQueuePurge(info);
        }
        public async ValueTask<int> QueueDeleteAsync(string queueName, bool ifUnused = false, bool ifEmpty = false)
        {
            await _semafore.WaitAsync().ConfigureAwait(false);
            _purgeOrDeleteSrc = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new QueueDeleteInfo(queueName, ifUnused, ifEmpty, false);
            await SendQueueDelete(info).ConfigureAwait(false);
            var result = await _purgeOrDeleteSrc.Task.ConfigureAwait(false);
            _queues.Remove(queueName);
            _semafore.Release();
            return result;
        }
        public async ValueTask QueueDeleteNoWaitAsync(string queueName, bool ifUnused = false, bool ifEmpty = false)
        {
            var info = new QueueDeleteInfo(queueName, ifUnused, ifEmpty, true);
            await SendQueueDelete(info).ConfigureAwait(false);
            _queues.Remove(queueName);
        }
    }
}

