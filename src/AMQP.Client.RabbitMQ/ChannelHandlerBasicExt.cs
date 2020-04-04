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
            await handler.Writer.SendBasicConsumeAsync(consumer.Channel.ChannelId, consumer.Consume).ConfigureAwait(false);
            var tag = await data.ConsumeTcs.Task.ConfigureAwait(false);
            if (!tag.Equals(consumer.Consume.ConsumerTag))
            {
                RabbitMQExceptionHelper.ThrowIfConsumeOkTagMissmatch(consumer.Consume.ConsumerTag, tag);
            }
            data.Consumers.Add(tag, consumer);
        }
    }
}
