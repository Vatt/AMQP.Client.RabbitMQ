using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol.Info.Exchange;
using System.Collections.Generic;
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
    public class ExchangeDeclareBuilder
    {
        private ExchangeInfo _info;
        private readonly ExchangeHandler _handler;
        internal ExchangeDeclareBuilder(ExchangeHandler handler,short channelId,string name, string type)
        {
            _handler = handler;
            _info = new ExchangeInfo(channelId, name, type, false, false, false, false, false, null); 
        }
        public ExchangeDeclareBuilder Durable(bool durable)
        {
            _info.Durable = durable;
            return this;
        }
        public ExchangeDeclareBuilder Durable()
        {
            _info.Durable = true;
            return this;
        }
        public ExchangeDeclareBuilder AutoDelete(bool autoDelete)
        {
            _info.AutoDelete = autoDelete;
            return this;
        }
        public ExchangeDeclareBuilder AutoDelete()
        {
            _info.AutoDelete = true;
            return this;
        }
        public ExchangeDeclareBuilder WithArgument(string key, object value)
        {
            if (_info.Arguments == null)
            {
                _info.Arguments = new Dictionary<string, object>();
            }
            _info.Arguments[key] = value;
            return this;
        }
        public ExchangeDeclareBuilder WithArguments(Dictionary<string, object> arguments)
        {
            _info.Arguments = arguments;
            return this;
        }
        public async Task<bool> DeclareAsync()
        {
            await _handler.WriteExchangeDeclareAsync(_info);
            return default;
        }
        public Task DeclareNoWaitAsync()
        {
            _info.NoWait = true;
            return default;
        }
        public Task<bool> DeclarePassiveAsync()
        {
            _info.Passive = true;
            _info.AutoDelete = true;
            return default;
        }
    }
}
