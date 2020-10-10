using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AMQP.Client.RabbitMQ.Consumer;
using AMQP.Client.RabbitMQ.Internal;
using AMQP.Client.RabbitMQ.Protocol;
using AMQP.Client.RabbitMQ.Protocol.Common;
using AMQP.Client.RabbitMQ.Protocol.Methods.Basic;

namespace AMQP.Client.RabbitMQ
{
    internal static class RabbitMQSessionBasicExt
    {
        internal static async Task ConsumerStartAsync(this RabbitMQSession session, RabbitMQConsumer consumer)
        {
            var data = session.GetChannelData(consumer.Channel.ChannelId);
            if (!consumer.Conf.NoWait)
            {
                data.ConsumeTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            await session.Writer.WriteAsync(ProtocolWriters.BasicConsumeWriter, consumer.Conf).ConfigureAwait(false);
            if (!consumer.Conf.NoWait)
            {
                var tag = await data.ConsumeTcs.Task.ConfigureAwait(false);
                if (!tag.Equals(consumer.Conf.ConsumerTag))
                {
                    RabbitMQExceptionHelper.ThrowIfConsumeOkTagMissmatch(consumer.Conf.ConsumerTag, tag);
                }
                consumer.IsClosed = false;
            }
            data.Consumers.Add(consumer.Conf.ConsumerTag, consumer);
        }

        internal static async Task QoS(this RabbitMQSession session, RabbitMQChannel channel, QoSInfo qos)
        {
            var data = session.GetChannelData(channel.ChannelId);
            data.CommonTcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            await session.Writer.WriteAsync(ProtocolWriters.BasicQoSWriter,qos).ConfigureAwait(false);
            await data.CommonTcs.Task.ConfigureAwait(false);
        }
    }
}