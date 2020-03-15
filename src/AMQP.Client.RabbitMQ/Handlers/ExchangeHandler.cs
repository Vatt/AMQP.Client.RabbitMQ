using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Framing;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ.Handlers
{
    public static class ExchangeType
    {
        public const string Direct = "direct";
        public const string Fanout = "fanout";
        public const string Headers = "headers";
        public const string Topic = "topic";

    }
    internal class ExchangeHandler
    {
        private readonly RabbitMQProtocol _protocol;
        private readonly ushort _channelId;
        private readonly SemaphoreSlim _semafore;
        private Dictionary<string, ExchangeInfo> _exchanges;
        private TaskCompletionSource<bool> _declareOkSrc;
        private TaskCompletionSource<bool> _deleteOkSrc;
        public ExchangeHandler(ushort channelId, RabbitMQProtocol protocol)
        {
            _exchanges = new Dictionary<string, ExchangeInfo>();
            _semafore = new SemaphoreSlim(1);
            _channelId = channelId;
            _protocol = protocol;
        }
        public async ValueTask HandleMethodAsync(MethodHeader method)
        {
            Debug.Assert(method.ClassId == 40);
            switch (method.MethodId)
            {
                case 11: //declare-ok
                    {
                        _declareOkSrc.SetResult(await _protocol.ReadExchangeDeclareOk().ConfigureAwait(false));
                        break;
                    }
                case 21:
                    {
                        _deleteOkSrc.SetResult(await _protocol.ReadExchangeDeleteOk().ConfigureAwait(false));
                        break;
                    }
                default:
                    throw new Exception($"{nameof(ExchangeHandler)}.HandleMethodAsync :cannot read frame (class-id,method-id):({method.ClassId},{method.MethodId})");

            }
        }
        public ValueTask<bool> DeclareAsync(string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            var info = new ExchangeInfo(name, type, durable: durable, autoDelete: autoDelete, arguments: arguments);
            return DeclarePrivateAsync(info);
        }
        public ValueTask DeclareNoWaitAsync(string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            var info = new ExchangeInfo(name, type, durable: durable, autoDelete: autoDelete, nowait: true, arguments: arguments);
            return _protocol.SendExchangeDeclareAsync(_channelId, info);
        }
        public ValueTask<bool> DeclarePassiveAsync(string name, string type, bool durable, bool autoDelete, Dictionary<string, object> arguments = null)
        {
            var info = new ExchangeInfo(name, type, durable: durable, autoDelete: autoDelete, passive: true, arguments: arguments);
            return DeclarePrivateAsync(info);
        }
        private async ValueTask<bool> DeclarePrivateAsync(ExchangeInfo info)
        {
            await _semafore.WaitAsync().ConfigureAwait(false);
            _declareOkSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            await _protocol.SendExchangeDeclareAsync(_channelId, info).ConfigureAwait(false);
            var result = await _declareOkSrc.Task.ConfigureAwait(false);
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
            await _semafore.WaitAsync().ConfigureAwait(false);
            _deleteOkSrc = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var info = new ExchangeDeleteInfo(name, ifUnused);
            await _protocol.SendExchangeDeleteAsync(_channelId, info).ConfigureAwait(false);
            var result = await _deleteOkSrc.Task.ConfigureAwait(false);
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
        public ValueTask DeleteNoWaitAsync(string name, bool ifUnused = false)
        {
            var info = new ExchangeDeleteInfo(name, ifUnused);
            _exchanges.Remove(info.Name);
            return _protocol.SendExchangeDeleteAsync(_channelId, info);


        }
    }
}
