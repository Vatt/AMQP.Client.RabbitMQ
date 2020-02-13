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
    public class QueueHandler: QueueReaderWriter
    {
        private readonly SemaphoreSlim _semafore;
        private Dictionary<string, QueueInfo> _queues;
        private TaskCompletionSource<QueueDeclareOk> _declareOkSrc;
        private TaskCompletionSource<bool> _deleteOkSrc;
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
                       _declareOkSrc.SetResult(await ReadDeclareOk());
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
    }
}
