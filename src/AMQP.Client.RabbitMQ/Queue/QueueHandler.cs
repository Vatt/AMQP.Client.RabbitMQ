using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Queue
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
                       _declareOkSrc.SetResult(await ReadQueueDeclareOk());
                        break;
                    }
                case 21://bind-ok
                    {
                        _commonSrc.SetResult(await ReadBindOkUnbindOk());
                        break;
                    }
                case 51://unbind-ok
                    {
                        _commonSrc.SetResult(await ReadBindOkUnbindOk());
                        break;
                    }
                case 31://purge-ok
                    {
                        _purgeOrDeleteSrc.SetResult(await ReadQueuePurgeOkDeleteOk());
                        break;
                    }
                case 41: //delete-ok
                    {
                        _purgeOrDeleteSrc.SetResult(await ReadQueuePurgeOkDeleteOk());
                        break;
                    }
                default:
                    throw new Exception($"{nameof(QueueHandler)}.HandleMethodAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        public async ValueTask<QueueDeclareOk> DeclareAsync(string name, bool durable, bool exclusive,bool autoDelete, Dictionary<string, object> arguments)
        {
            await _semafore.WaitAsync();
            _declareOkSrc = new TaskCompletionSource<QueueDeclareOk>();
            var info = new QueueInfo(name, durable, exclusive, autoDelete, arguments: arguments);
            await SendQueueDeclare(info);
            var okInfo = await _declareOkSrc.Task;
            _queues.Add(okInfo.Name, info);
            _semafore.Release();
            return okInfo;
        }
        public async ValueTask DeclareNoWaitAsync(string name, bool durable, bool exclusive, bool autoDelete, Dictionary<string, object> arguments)
        {
            var info = new QueueInfo(name, durable, exclusive, autoDelete, nowait:true, arguments: arguments);
            await SendQueueDeclare(info);
        }
        public async ValueTask<QueueDeclareOk> DeclarePassiveAsync(string name)
        {
            await _semafore.WaitAsync();
            _declareOkSrc = new TaskCompletionSource<QueueDeclareOk>();
            var info = new QueueInfo(name);
            await SendQueueDeclare(info);
            var okInfo = await _declareOkSrc.Task;
            _queues.Add(okInfo.Name, info);
            _semafore.Release();
            return okInfo;
        }
        public async ValueTask<QueueDeclareOk> DeclareQuorumAsync(string name)
        {
            await _semafore.WaitAsync();
            _declareOkSrc = new TaskCompletionSource<QueueDeclareOk>();
            var info = new QueueInfo(name,true, arguments:new Dictionary<string, object> {{ "x-queue-type", "quorum" }});
            await SendQueueDeclare(info);
            var okInfo = await _declareOkSrc.Task;
            _queues.Add(okInfo.Name, info);
            _semafore.Release();
            return okInfo;
        }
        public async ValueTask<bool> QueueBindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            await _semafore.WaitAsync();
            _commonSrc = new TaskCompletionSource<bool>();
            var info = new QueueBindInfo(queueName, exchangeName, routingKey, false, arguments);
            await SendQueueBind(info);
            var result = await _commonSrc.Task;
            _semafore.Release();
            return result;
        }

        public async ValueTask QueueBindNoWaitAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            var info = new QueueBindInfo(queueName, exchangeName, routingKey, true, arguments);
            await SendQueueBind(info);
        }
        public async ValueTask<bool> QueueUnbindAsync(string queueName, string exchangeName, string routingKey = "", Dictionary<string, object> arguments = null)
        {
            await _semafore.WaitAsync();
            _commonSrc = new TaskCompletionSource<bool>();
            var info = new QueueUnbindInfo(queueName, exchangeName, routingKey, arguments);
            await SendQueueUnbind(info);
            var result = await _commonSrc.Task;
            _semafore.Release();
            return result;
        }
        public async ValueTask<int> QueuePurgeAsync(string queueName)
        {
            await _semafore.WaitAsync();
            _purgeOrDeleteSrc = new TaskCompletionSource<int>();
            var info = new QueuePurgeInfo(queueName, false);
            await SendQueuePurge(info);
            var result = await _purgeOrDeleteSrc.Task;
            _semafore.Release();
            return result;
        }

        public async ValueTask QueuePurgeNoWaitAsync(string queueName)
        {
            var info = new QueuePurgeInfo(queueName, true);
            await SendQueuePurge(info);
        }
        public async ValueTask<int> QueueDeleteAsync(string queueName, bool ifUnused = false, bool ifEmpty = false)
        {
            await _semafore.WaitAsync();
            _purgeOrDeleteSrc = new TaskCompletionSource<int>();
            var info = new QueueDeleteInfo(queueName, ifUnused, ifEmpty, false);
            await SendQueueDelete(info);
            var result = await _purgeOrDeleteSrc.Task;
            _queues.Remove(queueName);
            _semafore.Release();
            return result;
        }
        public async ValueTask QueueDeleteNoWaitAsync(string queueName, bool ifUnused = false, bool ifEmpty = false)
        {
            var info = new QueueDeleteInfo(queueName, ifUnused, ifEmpty, true);
            await SendQueueDelete(info);
            _queues.Remove(queueName);
        }
    }
}

