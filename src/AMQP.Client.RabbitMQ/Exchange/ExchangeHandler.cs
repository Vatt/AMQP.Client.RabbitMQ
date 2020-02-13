using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Exchange
{
    public static class ExchangeType
    {
        public const string Direct = "direct";
        public const string Fanout = "fanout";
        public const string Headers = "headers";
        public const string Topic = "topic";

    }
    internal class ExchangeHandler : ExchangeReaderWriter
    {
        private readonly ushort _channelId;
        private readonly SemaphoreSlim _semafore;
        private Dictionary<string, ExchangeInfo> _exchanges;
        private TaskCompletionSource<bool> _declareOkSrc;
        private TaskCompletionSource<bool> _deleteOkSrc;
        public ExchangeHandler(ushort channelId, RabbitMQProtocol protocol):base(protocol)
        {
            _exchanges = new Dictionary<string, ExchangeInfo>();
            _semafore = new SemaphoreSlim(1);
            _channelId = channelId;
        }
        public async ValueTask HandleMethodAsync(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 40);
            switch (method.MethodId)
            {
                case 11: //declare-ok
                    {
                        _declareOkSrc.SetResult(await ReadExchangeDeclareOk());
                        break;
                    }
                case 21:
                    {
                        _deleteOkSrc.SetResult(await ReadExchangeDeleteOk());
                        break;
                    }
                default:
                    throw new Exception($"{nameof(ExchangeHandler)}.HandleMethodAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        public async ValueTask<bool> DeclareAsync(string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            var info = new ExchangeInfo(_channelId, name, type, durable: durable, autoDelete: autoDelete, arguments: arguments);
            return await DeclarePrivateAsync(info);
        }
        public async ValueTask DeclareNoWaitAsync(string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            var info = new ExchangeInfo(_channelId, name, type, durable: durable, autoDelete: autoDelete, nowait: true, arguments: arguments);
            await SendExchangeDeclareAsync(info);
        }
        public async ValueTask<bool> DeclarePassiveAsync(string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            var info = new ExchangeInfo(_channelId, name, type, durable: durable, autoDelete: autoDelete, passive: true, arguments: arguments);
            return await DeclarePrivateAsync(info);
        }
        private async ValueTask<bool> DeclarePrivateAsync(ExchangeInfo info)
        {
            await _semafore.WaitAsync();
            _declareOkSrc = new TaskCompletionSource<bool>();
            
            await SendExchangeDeclareAsync(info);
            var result = await _declareOkSrc.Task;
            if (result)
            {
                _exchanges.Add(info.Name, info);
            }
            else
            {
                //TODO: сделать что нибудь
            }
            _semafore.Release();
            return result;
        }
        public async ValueTask<bool> DeleteAsync(string name, bool ifUnused = false)
        {
            await _semafore.WaitAsync();
            _deleteOkSrc = new TaskCompletionSource<bool>();
            var info = new ExchangeDeleteInfo(_channelId, name, ifUnused);
            await SendExchangeDeleteAsync(info);
            var result = await _deleteOkSrc.Task;
            if (result)
            {
                _exchanges.Remove(info.Name);
            }
            else
            {
                //TODO: сделать что нибудь
            }
            _semafore.Release();
            return result;
        }
        public async ValueTask DeleteNoWaitAsync(string name, bool ifUnused = false)
        {
            var info = new ExchangeDeleteInfo(_channelId, name, ifUnused);
            await SendExchangeDeleteAsync(info);
            _exchanges.Remove(info.Name);

        }
    }
}
