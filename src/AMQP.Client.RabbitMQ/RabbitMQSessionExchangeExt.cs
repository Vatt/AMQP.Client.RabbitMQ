using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;

namespace AMQP.Client.RabbitMQ
{
    internal static class RabbitMQSessionExchangeExt
    {
        public static async ValueTask ExchangeDeclareAsync(this RabbitMQSession session, RabbitMQChannel channel, ExchangeDeclare exchange)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            if (exchange.NoWait)
            {
                await session.Writer.WriteAsync(ProtocolWriters.ExchangeDeclareWriter, exchange).ConfigureAwait(false);
                data.Exchanges.Add(exchange.Name, exchange);
                return;
            }
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await session.Writer.WriteAsync(ProtocolWriters.ExchangeDeclareWriter, exchange).ConfigureAwait(false);

            await src.CommonTcs.Task.ConfigureAwait(false);
            data.Exchanges.Add(exchange.Name, exchange);
        }
        public static async ValueTask ExchangeDeleteAsync(this RabbitMQSession session, RabbitMQChannel channel, ExchangeDelete exchange)
        {
            session.Channels.TryGetValue(channel.ChannelId, out var src);
            var data = session.GetChannelData(channel.ChannelId);
            if (exchange.NoWait)
            {
                await session.Writer.WriteAsync(ProtocolWriters.ExchangeDeleteWriter, exchange).ConfigureAwait(false);
                data.Exchanges.Remove(exchange.Name);
                return;
            }
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await session.Writer.WriteAsync(ProtocolWriters.ExchangeDeleteWriter, exchange).ConfigureAwait(false);
            await src.CommonTcs.Task.ConfigureAwait(false);
            data.Exchanges.Remove(exchange.Name);
        }
    }
}
