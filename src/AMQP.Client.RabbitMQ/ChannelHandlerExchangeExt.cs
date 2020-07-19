﻿using AMQP.Client.RabbitMQ.Protocol.Methods.Exchange;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    internal static class ChannelHandlerExchangeExt
    {
        public static async ValueTask ExchangeDeclareAsync(this ChannelHandler handler, RabbitMQChannel channel, ExchangeDeclare exchange)
        {
            handler.ChannelsWaitSrc.TryGetValue(channel.ChannelId, out var src);
            var data = handler.GetChannelData(channel.ChannelId);
            if (exchange.NoWait)
            {
                await handler.Writer.SendExchangeDeclareAsync(channel.ChannelId, exchange).ConfigureAwait(false);
                data.Exchanges.Add(exchange.Name, exchange);
                return;
            }
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await handler.Writer.SendExchangeDeclareAsync(channel.ChannelId, exchange).ConfigureAwait(false);

            await src.CommonTcs.Task.ConfigureAwait(false);
            data.Exchanges.Add(exchange.Name, exchange);
        }
        public static async ValueTask ExchangeDeleteAsync(this ChannelHandler handler, RabbitMQChannel channel, ExchangeDelete exchange)
        {
            handler.ChannelsWaitSrc.TryGetValue(channel.ChannelId, out var src);
            var data = handler.GetChannelData(channel.ChannelId);
            if (exchange.NoWait)
            {
                await handler.Writer.SendExchangeDeleteAsync(channel.ChannelId, exchange).ConfigureAwait(false);
                data.Exchanges.Remove(exchange.Name);
                return;
            }
            src.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await handler.Writer.SendExchangeDeleteAsync(channel.ChannelId, exchange).ConfigureAwait(false);

            await src.CommonTcs.Task.ConfigureAwait(false);
            data.Exchanges.Remove(exchange.Name);
        }
    }
}
