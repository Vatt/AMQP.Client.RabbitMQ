using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    internal static class ChannelHandlerExchangeExt
    {
        public static async ValueTask ExchangeDeclareAsync(this ChannelHandler handler, RabbitMQChannel channel, ExchangeDeclare exchange)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await handler.Writer.SendExchangeDeclareAsync(channel.ChannelId, exchange).ConfigureAwait(false);
            if (exchange.NoWait)
            {
                data.Exchanges.Add(exchange.Name, exchange);
                return;
            }
            await data.CommonTcs.Task.ConfigureAwait(false);
            data.Exchanges.Add(exchange.Name, exchange);
        }
        public static async ValueTask ExchangeDeleteAsync(this ChannelHandler handler, RabbitMQChannel channel, ExchangeDelete exchange)
        {
            handler.Channels.TryGetValue(channel.ChannelId, out var data);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await handler.Writer.SendExchangeDeleteAsync(channel.ChannelId, exchange).ConfigureAwait(false);
            if (exchange.NoWait)
            {
                data.Exchanges.Remove(exchange.Name);
                return;
            }
            await data.CommonTcs.Task.ConfigureAwait(false);
            data.Exchanges.Remove(exchange.Name);
        }
    }
}
