using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;
using System.Threading.Tasks;

namespace AMQP.Client.RabbitMQ
{
    internal static class ChannelHandlerBasicExt
    {
        internal static async Task ConsumerStartAsync(this ChannelHandler handler, RabbitMQConsumer consumer)
        {
            var data = handler.GetChannelData(consumer.Channel.ChannelId);
            data.ConsumeTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            await handler.Writer.SendBasicConsumeAsync(consumer.Channel.ChannelId, consumer.Conf).ConfigureAwait(false);
            var tag = await data.ConsumeTcs.Task.ConfigureAwait(false);
            if (!tag.Equals(consumer.Conf.ConsumerTag))
            {
                RabbitMQExceptionHelper.ThrowIfConsumeOkTagMissmatch(consumer.Conf.ConsumerTag, tag);
            }
            data.Consumers.Add(tag, consumer);
        }
        internal static async Task QoS(this ChannelHandler handler, RabbitMQChannel channel, QoSInfo qos)
        {
            var data = handler.GetChannelData(channel.ChannelId);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await handler.Writer.SendBasicQoSAsync(channel.ChannelId, ref qos).ConfigureAwait(false);
            await data.CommonTcs.Task.ConfigureAwait(false);
        }
    }
}
